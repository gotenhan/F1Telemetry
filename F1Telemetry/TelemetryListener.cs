using System;
using System.Buffers;
using System.CodeDom;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using log4net;

namespace F1Telemetry
{
    class TelemetryListener: IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Subject<TelemetryFrame> _framesSubject;

        public TelemetryListener()
        {
            _framesSubject = new Subject<TelemetryFrame>();
            Frames = _framesSubject.AsObservable();
        }

        public IObservable<TelemetryFrame> Frames;
        private volatile bool _listening;
        private CancellationTokenSource _cts;
        private static readonly int HeaderSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PacketHeader));

        public void StartListening(int port)
        {
            if (!_listening)
            {
                Logger.Info("Start listening");
                _cts = new CancellationTokenSource();
                var pipe = new Pipe();
                var networkThread = new Thread(DoStartListening);
                networkThread.Name = "Network thread";
                var parsingThread = new Thread(DoReadMessages);
                parsingThread.Name = "Parsing thread";

                _listening = true;
                parsingThread.Start((pipe.Reader, _cts.Token));
                networkThread.Start((port, pipe.Writer, _cts.Token));
            }
            else
            {
                Logger.Info("Already listening");
            }
        }

        private async void DoReadMessages(object o)
        {
            try
            {
                var (reader, ct) = (ValueTuple<PipeReader,CancellationToken>) o;
                while (_listening)
                {
                    var msg = await reader.ReadAsync(ct);
                    if (msg.IsCompleted || msg.IsCanceled)
                    {
                        break;
                    }

                    if (msg.Buffer.Length < HeaderSize)
                    {
                        reader.AdvanceTo(msg.Buffer.Start);
                        continue;
                    }

                    var header = ReadHeader(msg);
                    var requiredMessageSize = PacketHeader.PacketSizes[(int)header.PacketId];
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occured while parsing messages", ex);

            }
            finally
            {
                _listening = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PacketHeader ReadHeader(ReadResult msg)
        {
            Span<byte> headerSpan = stackalloc byte[HeaderSize];
            msg.Buffer.Slice(HeaderSize).CopyTo(headerSpan);
            return MemoryMarshal.Read<PacketHeader>(headerSpan);
        }

        public void StopListening()
        {
            if (_listening)
            {
                Logger.Info("Stopping listening");
                _listening = false;
                _cts?.Cancel();
            }
        }

        private async void DoStartListening(object o)
        {
            try
            {
                var (port, writer, ct) = (ValueTuple<int, PipeWriter, CancellationToken>) o;
                using (var udpClient = new UdpClient(port))
                {
                    while (_listening)
                    {
                        var result = await udpClient.ReceiveAsync();

                        if (result.Buffer.Length < HeaderSize)
                        {
                            Logger.Warn($"Invalid message size, could not read header. Size: {result.Buffer.Length}");
                            continue;
                        }

                        var header = MemoryMarshal.Read<PacketHeader>(result.Buffer.AsSpan());
                        if (!InitialValidate(header, result))
                        {
                            continue;
                        }

                        Logger.Debug($"Received header {header}");

                        await writer.WriteAsync(result.Buffer, _cts.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occured while receiving data from network", ex);
            }
            finally
            {
                _listening = false;
            }
        }

        private static bool InitialValidate(PacketHeader header, UdpReceiveResult result)
        {
            if (header.PacketFormat != 2018)
            {
                Logger.Warn($"Unsupported packet format: {header.PacketFormat}");
                return false;
            }

            if (header.PacketId < 0 || (int) header.PacketId >= PacketHeader.PacketSizes.Length)
            {
                Logger.Warn($"Unsupported packet ID: {header.PacketId}");
                return false;
            }

            var expectedPacketHeader = PacketHeader.PacketSizes[(int) header.PacketId];
            var expectedHeaderSize = HeaderSize + expectedPacketHeader;
            if (result.Buffer.Length < expectedHeaderSize)
            {
                Logger.Warn( $"Size of the message does not match expected size. Actual: {result.Buffer.Length}, Expected: {expectedHeaderSize} ({HeaderSize} for header + {expectedHeaderSize} for packet {((PacketHeader.PacketType) header.PacketId).ToString()}");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _framesSubject?.Dispose();
            _listening = false;
        }
    }

    class TelemetryFrame
    {
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    struct PacketHeader
    {
        public ushort PacketFormat;
        public byte PacketVersion;
        public PacketType PacketId;
        public ulong SessionUID;
        public float SessionTime;
        public uint FrameIdentifier;
        public byte PlayerCarIndex;

        public enum PacketType : byte
        {
            Motion = 0,
            Session = 1,
            LapData = 2,
            Event = 3,
            Participants = 4,
            CarSetups = 5,
            CarTelemetry = 6,
            CarStatus = 7
        }

        public static readonly int[] PacketSizes = {1341, 147, 841, 25, 1082, 841, 1085, 1061};

        public override string ToString()
        {
            return $"{nameof(PacketFormat)}: {PacketFormat}, {nameof(PacketVersion)}: {PacketVersion}, {nameof(PacketId)}: {PacketId}, {nameof(SessionUID)}: {SessionUID}, {nameof(SessionTime)}: {SessionTime}, {nameof(FrameIdentifier)}: {FrameIdentifier}, {nameof(PlayerCarIndex)}: {PlayerCarIndex}";
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MarshalZone
    {
        float ZoneStart;   // Fraction (0..1) of way through the lap the marshal zone starts
        sbyte ZoneFlag;    // -1 = invalid/unknown, 0 = none, 1 = green, 2 = blue, 3 = yellow, 4 = red

        public override string ToString()
        {
            return $"{nameof(ZoneStart)}: {ZoneStart}, {nameof(ZoneFlag)}: {ZoneFlag}";
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PacketSessionData
    {
        public PacketHeader Header;                  // Header
        
        public byte Weather;                // Weather - 0 = clear, 1 = light cloud, 2 = overcast
                                             // 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte TrackTemperature;        // Track temp. in degrees celsius
        public sbyte AirTemperature;          // Air temp. in degrees celsius
        public byte TotalLaps;              // Total number of laps in this race
        public ushort TrackLength;               // Track length in metres
        public byte SessionType;            // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P
                                             // 5 = Q1, 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ
                                            // 10 = R, 11 = R2, 12 = Time Trial
        public sbyte TrackId;                 // -1 for unknown, 0-21 for tracks, see appendix
        public byte Era;                    // Era, 0 = modern, 1 = classic
        public ushort SessionTimeLeft;       // Time left in session in seconds
        public ushort SessionDuration;       // Session duration in seconds
        public byte PitSpeedLimit;          // Pit speed limit in kilometres per hour
        public byte GamePaused;               // Whether the game is paused
        public byte IsSpectating;           // Whether the player is spectating
        public byte SpectatorCarIndex;      // Index of the car being spectated
        public byte SliProNativeSupport;    // SLI Pro support, 0 = inactive, 1 = active
        public byte NumMarshalZones;            // Number of marshal zones to follow
        public byte[] MarshalZonesRaw;         // List of marshal zones – max 21
        public byte SafetyCarStatus;          // 0 = no safety car, 1 = full safety car
                                            // 2 = virtual safety car
        public byte NetworkGame;              // 0 = offline, 1 = online

        public MarshalZone[] MarshalZones => MemoryMarshal.Cast<byte, MarshalZone>(MarshalZonesRaw.AsSpan()).ToArray();

        public override string ToString()
        {
            return $"{nameof(Header)}: {Header}, {nameof(Weather)}: {Weather}, {nameof(TrackTemperature)}: {TrackTemperature}, {nameof(AirTemperature)}: {AirTemperature}, {nameof(TotalLaps)}: {TotalLaps}, {nameof(TrackLength)}: {TrackLength}, {nameof(SessionType)}: {SessionType}, {nameof(TrackId)}: {TrackId}, {nameof(Era)}: {Era}, {nameof(SessionTimeLeft)}: {SessionTimeLeft}, {nameof(SessionDuration)}: {SessionDuration}, {nameof(PitSpeedLimit)}: {PitSpeedLimit}, {nameof(GamePaused)}: {GamePaused}, {nameof(IsSpectating)}: {IsSpectating}, {nameof(SpectatorCarIndex)}: {SpectatorCarIndex}, {nameof(SliProNativeSupport)}: {SliProNativeSupport}, {nameof(NumMarshalZones)}: {NumMarshalZones}, {nameof(SafetyCarStatus)}: {SafetyCarStatus}, {nameof(NetworkGame)}: {NetworkGame}, {nameof(MarshalZones)}: {MarshalZones}";
        }
    };
}

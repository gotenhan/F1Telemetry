using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using F1TelemetryNetCore.Packets;
using log4net;

namespace F1TelemetryNetCore
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
        private Timer _timer;

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
            PipeReader reader = null;
            CancellationToken ct;
            try
            {
                (reader, ct) = (ValueTuple<PipeReader,CancellationToken>) o;
                while (_listening && !ct.IsCancellationRequested)
                {
                    var msg = await reader.ReadAsync(ct);
                    if (msg.IsCompleted || msg.IsCanceled)
                    {
                        break;
                    }

                    if (!PacketParser.TryReadFromBuffer(msg.Buffer, out PacketHeader header, out int headerBytesRead))
                    {
                        reader.AdvanceTo(msg.Buffer.Start, msg.Buffer.End);
                    }
                    else
                    {
                        switch (header.PacketId)
                        {
                            case PacketHeader.PacketType.Session:
                                ReadMessageAndAdvanceBuffer<PacketSessionData>();
                                break;
                            case PacketHeader.PacketType.CarSetups:
                                ReadMessageAndAdvanceBuffer<PacketCarSetupData>();
                                break;
                            case PacketHeader.PacketType.CarStatus:
                                ReadMessageAndAdvanceBuffer<PacketCarStatusData>();
                                break;
                            case PacketHeader.PacketType.CarTelemetry:
                                ReadMessageAndAdvanceBuffer<PacketCarTelemetryData>();
                                break;
                            case PacketHeader.PacketType.Event:
                                ReadMessageAndAdvanceBuffer<PacketEventData>();
                                break;
                            case PacketHeader.PacketType.LapData:
                                ReadMessageAndAdvanceBuffer<PacketLapData>();
                                break;
                            case PacketHeader.PacketType.Motion:
                                ReadMessageAndAdvanceBuffer<PacketMotionData>();
                                break;
                            case PacketHeader.PacketType.Participants:
                                ReadMessageAndAdvanceBuffer<PacketParticipantsData>();
                                break;
                            default:
                                throw new InvalidOperationException(
                                    $"Unsupported packet id {header.PacketId}, message should have been filtered before");
                        }
                    }

                    void ReadMessageAndAdvanceBuffer<T>() where T:struct
                    {
                        if (!PacketParser.TryReadFromBuffer<T>(msg.Buffer, out T sessionData, out int bytesRead))
                        {
                            reader.AdvanceTo(msg.Buffer.Start, msg.Buffer.End);
                            return;
                        }

                        Logger.Debug($"Successfully parsed {typeof(T).Name}: {sessionData}");
                        reader.AdvanceTo(msg.Buffer.GetPosition(bytesRead));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occured while parsing messages", ex);
            }
            finally
            {
                reader?.Complete();
                _listening = false;
            }
        }

        public void StopListening()
        {
            if (!_listening)
            {
                return;
            }

            Logger.Info("Stopping listening");
            _listening = false;
            _cts?.Cancel();
        }

        private async void DoStartListening(object o)
        {
            PipeWriter writer = null;
            CancellationToken ct;
            UdpClient udpClient = null;
            try
            {
                var port = 0;
                (port, writer, ct) = (ValueTuple<int, PipeWriter, CancellationToken>) o;
                using (udpClient = new UdpClient(port))
                {
                    _timer = new Timer(uc =>
                    {
                        if (!_listening || ct.IsCancellationRequested)
                        {
                            ((UdpClient)uc)?.Close();
                            _timer?.Dispose();
                        }

                    }, udpClient, 1000, 1000);

                    while (_listening && !ct.IsCancellationRequested)
                    {
                        var result = await udpClient.ReceiveAsync();
                        var buffer = result.Buffer;

                        if (buffer.Length == 0)
                            continue;

                        if (!PacketParser.InitialValidate(buffer))
                        {
                            continue;
                        }

                        await writer.WriteAsync(buffer, _cts.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occured while receiving data from network", ex);
            }
            finally
            {
                _timer?.Dispose();
                writer?.Complete();
                udpClient?.Close();
                _listening = false;
            }
        }

        public void Dispose()
        {
            _framesSubject?.Dispose();
            _listening = false;
        }
    }

    static class PacketParser
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool InitialValidate(byte[] buffer)
        {
            var header = MemoryMarshal.Read<PacketHeader>(buffer);
            if (buffer.Length < Unsafe.SizeOf<PacketHeader>())
            {
                Logger.Warn($"Invalid message size, could not read header. Size: {buffer.Length}");
                return false;
            }

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
            if (buffer.Length < expectedPacketHeader)
            {
                Logger.Warn( $"Size of the message does not match expected size. Actual: {buffer.Length}, Expected: {expectedPacketHeader}  for packet {((PacketHeader.PacketType) header.PacketId).ToString()}");
                return false;
            }

            Logger.Debug($"Received valid header {header}");
            return true;
        }
        public static unsafe bool TryReadFromBuffer<T>(ReadOnlySequence<byte> buffer, out T result, out int bytesRead) where T:struct
        {
            var packetSize = Unsafe.SizeOf<T>();
            if (buffer.Length < packetSize)
            {
                result = default;
                bytesRead = 0;
                return false;
            }

            if (buffer.FirstSpan.Length >= packetSize)
            {
                result = MemoryMarshal.Read<T>(buffer.FirstSpan.Slice(0, packetSize));
                bytesRead = packetSize;
                return true;
            }
            else
            {
                var slice = buffer.Slice(0, packetSize);
                Span<byte> tempBuffer = stackalloc byte[packetSize];
                slice.CopyTo(tempBuffer);

                result = MemoryMarshal.Read<T>(tempBuffer);
                bytesRead = packetSize;
                return true;
            }
        }

    }

    class TelemetryFrame
    {
    }

}

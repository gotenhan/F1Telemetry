using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace F1TelemetryNetCore.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct PacketSessionData
    {
        private const int MarshalZonesBufferSize = 21 * MarshalZone.Size;

        public PacketHeader Header;                  // Header
        public byte Weather;                // Weather - 0 = clear, 1 = light cloud, 2 = overcast // 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte TrackTemperature;        // Track temp. in degrees celsius
        public sbyte AirTemperature;          // Air temp. in degrees celsius
        public byte TotalLaps;              // Total number of laps in this race
        public ushort TrackLength;               // Track length in metres
        public byte SessionType;            // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P // 5 = Q1, 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ // 10 = R, 11 = R2, 12 = Time Trial
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
        public fixed byte MarshalZonesRaw[MarshalZonesBufferSize];         // List of marshal zones – max 21
        public byte SafetyCarStatus;          // 0 = no safety car, 1 = full safety car // 2 = virtual safety car
        public byte NetworkGame;              // 0 = offline, 1 = online

        public Span<MarshalZone> MarshalZones => new Span<MarshalZone>(Unsafe.AsPointer(ref MarshalZonesRaw[0]), MarshalZonesBufferSize);

        public override string ToString()
        {
            return
                $"{nameof(Header)}: {Header}, {nameof(Weather)}: {Weather}, {nameof(TrackTemperature)}: {TrackTemperature}, {nameof(AirTemperature)}: {AirTemperature}, {nameof(TotalLaps)}: {TotalLaps}, {nameof(TrackLength)}: {TrackLength}, {nameof(SessionType)}: {SessionType}, {nameof(TrackId)}: {TrackId}, {nameof(Era)}: {Era}, {nameof(SessionTimeLeft)}: {SessionTimeLeft}, {nameof(SessionDuration)}: {SessionDuration}, {nameof(PitSpeedLimit)}: {PitSpeedLimit}, {nameof(GamePaused)}: {GamePaused}, {nameof(IsSpectating)}: {IsSpectating}, {nameof(SpectatorCarIndex)}: {SpectatorCarIndex}, {nameof(SliProNativeSupport)}: {SliProNativeSupport}, {nameof(NumMarshalZones)}: {NumMarshalZones}, {nameof(SafetyCarStatus)}: {SafetyCarStatus}, {nameof(NetworkGame)}: {NetworkGame}, {nameof(MarshalZones)}: [{string.Join(";", MarshalZones.ToArray().Take(NumMarshalZones))}]";
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MarshalZone
    {
        public const int Size = sizeof(float) + sizeof(sbyte);

        float ZoneStart;   // Fraction (0..1) of way through the lap the marshal zone starts
        sbyte ZoneFlag;    // -1 = invalid/unknown, 0 = none, 1 = green, 2 = blue, 3 = yellow, 4 = red

        public override string ToString()
        {
            return $"{nameof(ZoneStart)}: {ZoneStart}, {nameof(ZoneFlag)}: {ZoneFlag}";
        }
    };
}
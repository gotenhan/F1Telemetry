using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace F1TelemetryNetCore.Packets
{
    unsafe struct PacketLapData
    {
        public PacketHeader Header; // Header

        private const int LapDataBufferSize = 20 * LapData.Size;
        private fixed byte LapDataRaw[LapDataBufferSize]; // Lap data for all cars on track

        public Span<LapData> LapDatas => new Span<LapData>(Unsafe.AsPointer(ref LapDataRaw[0]), LapDataBufferSize);

        public override string ToString()
        {
            return $"{nameof(Header)}: {Header}, {nameof(LapData)}: [{string.Join(";", LapDatas.ToArray())}]";
        }
    }

    struct LapData
    {
        public const int Size = 8 * sizeof(float) + 9;

        public float LastLapTime;           // Last lap time in seconds
        public float CurrentLapTime;        // Current time around the lap in seconds
        public float BestLapTime;           // Best lap time of the session in seconds
        public float Sector1Time;           // Sector 1 time in seconds
        public float Sector2Time;           // Sector 2 time in seconds
        public float LapDistance;           // Distance vehicle is around current lap in metres – could  be negative if line hasn’t been crossed yet
        public float TotalDistance;         // Total distance travelled in session in metres – could  be negative if line hasn’t been crossed yet
        public float SafetyCarDelta;        // Delta in seconds for safety car
        public byte CarPosition;           // Car race position
        public byte CurrentLapNum;         // Current lap number
        public byte PitStatus;             // 0 = none, 1 = pitting, 2 = in pit area
        public byte Sector;                // 0 = sector1, 1 = sector2, 2 = sector3
        public byte CurrentLapInvalid;     // Current lap invalid - 0 = valid, 1 = invalid
        public byte Penalties;             // Accumulated time penalties in seconds to be added
        public byte GridPosition;          // Grid position the vehicle started the race in
        public byte DriverStatus;          // Status of driver - 0 = in garage, 1 = flying lap  2 = in lap, 3 = out lap, 4 = on track
        public byte ResultStatus;          // Result status - 0 = invalid, 1 = inactive, 2 = active 3 = finished, 4 = disqualified, 5 = not classified  6 = retired

        public override string ToString()
        {
            return $"{nameof(LastLapTime)}: {LastLapTime}, {nameof(CurrentLapTime)}: {CurrentLapTime}, {nameof(BestLapTime)}: {BestLapTime}, {nameof(Sector1Time)}: {Sector1Time}, {nameof(Sector2Time)}: {Sector2Time}, {nameof(LapDistance)}: {LapDistance}, {nameof(TotalDistance)}: {TotalDistance}, {nameof(SafetyCarDelta)}: {SafetyCarDelta}, {nameof(CarPosition)}: {CarPosition}, {nameof(CurrentLapNum)}: {CurrentLapNum}, {nameof(PitStatus)}: {PitStatus}, {nameof(Sector)}: {Sector}, {nameof(CurrentLapInvalid)}: {CurrentLapInvalid}, {nameof(Penalties)}: {Penalties}, {nameof(GridPosition)}: {GridPosition}, {nameof(DriverStatus)}: {DriverStatus}, {nameof(ResultStatus)}: {ResultStatus}";
        }
    };
}

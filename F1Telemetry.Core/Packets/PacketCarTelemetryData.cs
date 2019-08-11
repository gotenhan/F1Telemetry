using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace F1TelemetryNetCore.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct PacketCarTelemetryData
    {
        public PacketHeader Header;                // Header

        private const int CarTelemetryDataBufferSize = 20 * CarTelemetryData.Size;
        private fixed byte CarTelemetryDataRaw[CarTelemetryDataBufferSize];
        public Span<CarTelemetryData> CarTelemetries => new Span<CarTelemetryData>(Unsafe.AsPointer(ref CarTelemetryDataRaw[0]), CarTelemetryDataBufferSize);

        public uint ButtonStatus;         // Bit flags specifying which buttons are being pressed currently - see appendices

        public override string ToString()
        {
            return $"{nameof(Header)}: {Header}, {nameof(ButtonStatus)}: {ButtonStatus}, {nameof(CarTelemetries)}: [{string.Join(";", CarTelemetries.ToArray())}]";
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct CarTelemetryData
    {
        public const int Size = 15 * sizeof(ushort) + 5 * sizeof(byte) + 2 * sizeof(sbyte) + 4 * sizeof(float);

        public ushort Speed;                      // Speed of car in kilometres per hour
        public byte Throttle;                   // Amount of throttle applied (0 to 100)
        public sbyte Steer;                      // Steering (-100 (full lock left) to 100 (full lock right))
        public byte Brake;                      // Amount of brake applied (0 to 100)
        public byte Clutch;                     // Amount of clutch applied (0 to 100)
        public sbyte Gear;                       // Gear selected (1-8, N=0, R=-1)
        public ushort EngineRPM;                  // Engine RPM
        public byte Drs;                        // 0 = off, 1 = on
        public byte RevLightsPercent;           // Rev lights indicator (percentage)
        private fixed ushort BrakesTemperatureRaw[4];       // Brakes temperature (celsius)
        private fixed ushort TyresSurfaceTemperatureRaw[4]; // Tyres surface temperature (celsius)
        private fixed ushort TyresInnerTemperatureRaw[4];   // Tyres inner temperature (celsius)
        public ushort EngineTemperature;          // Engine temperature (celsius)
        private fixed float TyresPressureRaw[4];           // Tyres pressure (PSI)

        public Span<ushort> BrakesTemperature => new Span<ushort>(Unsafe.AsPointer(ref BrakesTemperatureRaw[0]), 4 * sizeof(ushort));
        public Span<ushort> TyresSurfaceTemperature => new Span<ushort>(Unsafe.AsPointer(ref TyresSurfaceTemperatureRaw[0]), 4 * sizeof(ushort));
        public Span<ushort> TyresInnerTemperature => new Span<ushort>(Unsafe.AsPointer(ref TyresInnerTemperatureRaw[0]), 4 * sizeof(ushort));
        public Span<ushort> TyresPressure => new Span<ushort>(Unsafe.AsPointer(ref TyresPressureRaw[0]), 4 * sizeof(ushort));

        public override string ToString()
        {
            return $"{nameof(Speed)}: {Speed}, {nameof(Throttle)}: {Throttle}, {nameof(Steer)}: {Steer}, {nameof(Brake)}: {Brake}, "+
                   $"{nameof(Clutch)}: {Clutch}, {nameof(Gear)}: {Gear}, {nameof(EngineRPM)}: {EngineRPM}, "+
                   $"{nameof(Drs)}: {Drs}, {nameof(RevLightsPercent)}: {RevLightsPercent}, "+
                   $"{nameof(EngineTemperature)}: {EngineTemperature}, {nameof(BrakesTemperature)}: [{string.Join(";", BrakesTemperature.ToArray())}], "+
                   $"{nameof(TyresSurfaceTemperature)}: [{string.Join(";", TyresSurfaceTemperature.ToArray())}], {nameof(TyresInnerTemperature)}: [{string.Join(";", TyresInnerTemperature.ToArray())}], {nameof(TyresPressure)}: [{string.Join(";", TyresPressure.ToArray())}]";
        }
    };
}

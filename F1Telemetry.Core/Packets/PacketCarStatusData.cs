using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace F1TelemetryNetCore.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public unsafe struct PacketCarStatusData
    {
        private const int CarStatusBufferSize = 20 * CarStatusData.Size;
        public PacketHeader Header;            // Header

        private fixed byte CarStatusDataRaw[CarStatusBufferSize];
        public Span<CarStatusData> CarStatuses => new Span<CarStatusData>(Unsafe.AsPointer(ref CarStatusDataRaw[0]), CarStatusBufferSize);

        public override string ToString()
        {
            return $"{nameof(Header)}: {Header}, {nameof(CarStatuses)}: [{string.Join(";", CarStatuses.ToArray())}]";
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public unsafe struct CarStatusData
    {
        public const int Size = 23 * sizeof(byte) + 6 * sizeof(float) + 2 * sizeof(ushort) + sizeof(sbyte);

        public byte TractionControl;          // 0 (off) - 2 (high)
        public byte AntiLockBrakes;           // 0 (off) - 1 (on)
        public byte FuelMix;                  // Fuel mix - 0 = lean, 1 = standard, 2 = rich, 3 = max
        public byte FrontBrakeBias;           // Front brake bias (percentage)
        public byte PitLimiterStatus;         // Pit limiter status - 0 = off, 1 = on
        public float FuelInTank;               // Current fuel mass
        public float FuelCapacity;             // Fuel capacity
        public ushort MaxRPM;                   // Cars max RPM, point of rev limiter
        public ushort IdleRPM;                  // Cars idle RPM
        public byte MaxGears;                 // Maximum number of gears
        public byte DrsAllowed;               // 0 = not allowed, 1 = allowed, -1 = unknown
        private fixed byte TyresWearRaw[4];             // Tyre wear percentage
        public byte TyreCompound;             // Modern - 0 = hyper soft, 1 = ultra soft
                                          // 2 = super soft, 3 = soft, 4 = medium, 5 = hard
                                          // 6 = super hard, 7 = inter, 8 = wet
                                          // Classic - 0-6 = dry, 7-8 = wet
        private fixed byte TyresDamageRaw[4];           // Tyre damage (percentage)
        public byte FrontLeftWingDamage;      // Front left wing damage (percentage)
        public byte FrontRightWingDamage;     // Front right wing damage (percentage)
        public byte RearWingDamage;           // Rear wing damage (percentage)
        public byte EngineDamage;             // Engine damage (percentage)
        public byte GearBoxDamage;            // Gear box damage (percentage)
        public byte ExhaustDamage;            // Exhaust damage (percentage)
        public sbyte VehicleFiaFlags;          // -1 = invalid/unknown, 0 = none, 1 = green
                                         // 2 = blue, 3 = yellow, 4 = red
        public float ErsStoreEnergy;           // ERS energy store in Joules
        public byte ErsDeployMode;            // ERS deployment mode, 0 = none, 1 = low, 2 = medium
                                          // 3 = high, 4 = overtake, 5 = hotlap
        public float ErsHarvestedThisLapMGUK;  // ERS energy harvested this lap by MGU-K
        public float ErsHarvestedThisLapMGUH;  // ERS energy harvested this lap by MGU-H
        public float ErsDeployedThisLap;       // ERS energy deployed this lap

        public Span<byte> TyresWear => new Span<byte>(Unsafe.AsPointer(ref TyresWearRaw[0]), 4 * sizeof(byte));
        public Span<byte> TyresDamage => new Span<byte>(Unsafe.AsPointer(ref TyresDamageRaw[0]), 4 * sizeof(byte));

        public override string ToString()
        {
            return $"{nameof(TractionControl)}: {TractionControl}, {nameof(AntiLockBrakes)}: {AntiLockBrakes}, {nameof(FuelMix)}: {FuelMix}, " +
                   $"{nameof(FrontBrakeBias)}: {FrontBrakeBias}, {nameof(PitLimiterStatus)}: {PitLimiterStatus}, " +
                   $"{nameof(FuelInTank)}: {FuelInTank}, {nameof(FuelCapacity)}: {FuelCapacity}, " +
                   $"{nameof(MaxRPM)}: {MaxRPM}, {nameof(IdleRPM)}: {IdleRPM}, {nameof(MaxGears)}: {MaxGears}, "+
                   $"{nameof(DrsAllowed)}: {DrsAllowed}, {nameof(TyreCompound)}: {TyreCompound}, "+
                   $"{nameof(FrontLeftWingDamage)}: {FrontLeftWingDamage}, {nameof(FrontRightWingDamage)}: {FrontRightWingDamage}, {nameof(RearWingDamage)}: {RearWingDamage}, "+
                   $"{nameof(EngineDamage)}: {EngineDamage}, {nameof(GearBoxDamage)}: {GearBoxDamage}, {nameof(ExhaustDamage)}: {ExhaustDamage}, "+
                   $"{nameof(VehicleFiaFlags)}: {VehicleFiaFlags}, "+
                   $"{nameof(ErsStoreEnergy)}: {ErsStoreEnergy}, {nameof(ErsDeployMode)}: {ErsDeployMode}, " + 
                   $"{nameof(ErsHarvestedThisLapMGUK)}: {ErsHarvestedThisLapMGUK}, {nameof(ErsHarvestedThisLapMGUH)}: {ErsHarvestedThisLapMGUH}, {nameof(ErsDeployedThisLap)}: {ErsDeployedThisLap}, "+
                   $"{nameof(TyresWear)}: [{string.Join(";", TyresWear.ToArray())}], {nameof(TyresDamage)}: [{string.Join(";", TyresDamage.ToArray())}]";
        }
    };
}

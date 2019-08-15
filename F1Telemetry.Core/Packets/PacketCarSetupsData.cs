using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace F1TelemetryNetCore.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public unsafe struct PacketCarSetupData
    {
        public PacketHeader Header;            // Header

        private const int CarSetupsBufferSize = 20 * CarSetupData.Size;
        private fixed byte CarSetupsRaw[CarSetupsBufferSize];

        public Span<CarSetupData> CarSetups => new Span<CarSetupData>(Unsafe.AsPointer(ref CarSetupsRaw[0]), CarSetupsBufferSize);

        public override string ToString()
        {
            return $"{nameof(Header)}: {Header}, [{nameof(CarSetups)}: {string.Join(";", CarSetups.ToArray())}]";
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct CarSetupData
    {
        public const int Size = 7 * sizeof(float) + 13;

        public byte FrontWing;                // Front wing aero
        public byte RearWing;                 // Rear wing aero
        public byte OnThrottle;               // Differential adjustment on throttle (percentage)
        public byte OffThrottle;              // Differential adjustment off throttle (percentage)
        public float FrontCamber;              // Front camber angle (suspension geometry)
        public float RearCamber;               // Rear camber angle (suspension geometry)
        public float FrontToe;                 // Front toe angle (suspension geometry)
        public float RearToe;                  // Rear toe angle (suspension geometry)
        public byte FrontSuspension;          // Front suspension
        public byte RearSuspension;           // Rear suspension
        public byte FrontAntiRollBar;         // Front anti-roll bar
        public byte RearAntiRollBar;          // Front anti-roll bar
        public byte FrontSuspensionHeight;    // Front ride height
        public byte RearSuspensionHeight;     // Rear ride height
        public byte BrakePressure;            // Brake pressure (percentage)
        public byte BrakeBias;                // Brake bias (percentage)
        public float FrontTyrePressure;        // Front tyre pressure (PSI)
        public float RearTyrePressure;         // Rear tyre pressure (PSI)
        public byte Ballast;                  // Ballast
        public float FuelLoad;                 // Fuel load

        public override string ToString()
        {
            return $"{nameof(FrontWing)}: {FrontWing}, {nameof(RearWing)}: {RearWing}, {nameof(OnThrottle)}: {OnThrottle}, {nameof(OffThrottle)}: {OffThrottle}, {nameof(FrontCamber)}: {FrontCamber}, {nameof(RearCamber)}: {RearCamber}, {nameof(FrontToe)}: {FrontToe}, {nameof(RearToe)}: {RearToe}, {nameof(FrontSuspension)}: {FrontSuspension}, {nameof(RearSuspension)}: {RearSuspension}, {nameof(FrontAntiRollBar)}: {FrontAntiRollBar}, {nameof(RearAntiRollBar)}: {RearAntiRollBar}, {nameof(FrontSuspensionHeight)}: {FrontSuspensionHeight}, {nameof(RearSuspensionHeight)}: {RearSuspensionHeight}, {nameof(BrakePressure)}: {BrakePressure}, {nameof(BrakeBias)}: {BrakeBias}, {nameof(FrontTyrePressure)}: {FrontTyrePressure}, {nameof(RearTyrePressure)}: {RearTyrePressure}, {nameof(Ballast)}: {Ballast}, {nameof(FuelLoad)}: {FuelLoad}";
        }
    };
}

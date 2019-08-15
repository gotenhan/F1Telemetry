using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace F1TelemetryNetCore.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PacketMotionData
    {
        public PacketHeader Header;               // Header

        private const int CarMotionDataBufferSize = 20 * Packets.CarMotionData.Size;
        private fixed byte CarMotionDataRaw[CarMotionDataBufferSize]; // Data for all cars on track

        // Extra player car ONLY data
        private fixed float SuspensionPositionRaw[4];       // Note: All wheel arrays have the following order:
        private fixed float SuspensionVelocityRaw[4];       // RL, RR, FL, FR
        private fixed float SuspensionAccelerationRaw[4];   // RL, RR, FL, FR
        private fixed float WheelSpeedRaw[4];               // Speed of each wheel
        private fixed float WheelSlipRaw[4];                // Slip ratio for each wheel
        public float LocalVelocityX;              // Velocity in local space
        public float LocalVelocityY;              // Velocity in local space
        public float LocalVelocityZ;              // Velocity in local space
        public float AngularVelocityX;            // Angular velocity x-component
        public float AngularVelocityY;            // Angular velocity y-component
        public float AngularVelocityZ;            // Angular velocity z-component
        public float AngularAccelerationX;        // Angular velocity x-component
        public float AngularAccelerationY;        // Angular velocity y-component
        public float AngularAccelerationZ;        // Angular velocity z-component
        public float FrontWheelsAngle;            // Current front wheels angle in radians

        public Span<CarMotionData> CarMotionDatas => new Span<CarMotionData>(Unsafe.AsPointer(ref CarMotionDataRaw[0]), CarMotionDataBufferSize);
        public Span<float> SuspensionPosition => new Span<float>(Unsafe.AsPointer(ref SuspensionPositionRaw[0]), 4 * sizeof(float));
        public Span<float> SuspensionVelocity => new Span<float>(Unsafe.AsPointer(ref SuspensionVelocityRaw[0]), 4 * sizeof(float));
        public Span<float> SuspensionAcceleration => new Span<float>(Unsafe.AsPointer(ref SuspensionAccelerationRaw[0]), 4 * sizeof(float));
        public Span<float> WheelSpeed => new Span<float>(Unsafe.AsPointer(ref WheelSpeedRaw[0]), 4 * sizeof(float));
        public Span<float> WheelSlip => new Span<float>(Unsafe.AsPointer(ref WheelSlipRaw[0]), 4 * sizeof(float));

        public override string ToString()
        {
            return
                $"{nameof(Header)}: {Header}, " +
                $"{nameof(CarMotionData)}: [{string.Join(";", CarMotionDatas.ToArray())}]" +
                $"{nameof(SuspensionPosition)}: [{string.Join(";", SuspensionPosition.ToArray())}], " +
                $"{nameof(SuspensionVelocity)}: [{string.Join(";", SuspensionVelocity.ToArray())}], " +
                $"{nameof(SuspensionAcceleration)}: [{SuspensionAcceleration.ToArray()}], " +
                $"{nameof(WheelSpeed)}: [{string.Join(";", WheelSpeed.ToArray())}], " +
                $"{nameof(WheelSlip)}: [{string.Join(";", WheelSlip.ToArray())}], " +
                $"{nameof(LocalVelocityX)}: {LocalVelocityX}, {nameof(LocalVelocityY)}: {LocalVelocityY}, {nameof(LocalVelocityZ)}: {LocalVelocityZ}, {nameof(AngularVelocityX)}: {AngularVelocityX}, " +
                $"{nameof(AngularVelocityY)}: {AngularVelocityY}, {nameof(AngularVelocityZ)}: {AngularVelocityZ}, " +
                $"{nameof(AngularAccelerationX)}: {AngularAccelerationX}, {nameof(AngularAccelerationY)}: {AngularAccelerationY}, " +
                $"{nameof(AngularAccelerationZ)}: {AngularAccelerationZ}, " +
                $"{nameof(FrontWheelsAngle)}: {FrontWheelsAngle}";
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarMotionData
    {
        public const int Size = 12 * sizeof(float) + 6 * sizeof(short);

        public float WorldPositionX;           // World space X position
        public float WorldPositionY;           // World space Y position
        public float WorldPositionZ;           // World space Z position
        public float WorldVelocityX;           // Velocity in world space X
        public float WorldVelocityY;           // Velocity in world space Y
        public float WorldVelocityZ;           // Velocity in world space Z
        public short WorldForwardDirX;         // World space forward X direction (normalised)
        public short WorldForwardDirY;         // World space forward Y direction (normalised)
        public short WorldForwardDirZ;         // World space forward Z direction (normalised)
        public short WorldRightDirX;           // World space right X direction (normalised)
        public short WorldRightDirY;           // World space right Y direction (normalised)
        public short WorldRightDirZ;           // World space right Z direction (normalised)
        public float GForceLateral;            // Lateral G-Force component
        public float GForceLongitudinal;       // Longitudinal G-Force component
        public float GForceVertical;           // Vertical G-Force component
        public float Yaw;                      // Yaw angle in radians
        public float Pitch;                    // Pitch angle in radians
        public float Roll;                     // Roll angle in radians

        public override string ToString()
        {
            return
                $"{nameof(WorldPositionX)}: {WorldPositionX}, {nameof(WorldPositionY)}: {WorldPositionY}, {nameof(WorldPositionZ)}: {WorldPositionZ}, " +
                $"{nameof(WorldVelocityX)}: {WorldVelocityX}, {nameof(WorldVelocityY)}: {WorldVelocityY}, {nameof(WorldVelocityZ)}: {WorldVelocityZ}, " +
                $"{nameof(WorldForwardDirX)}: {WorldForwardDirX}, {nameof(WorldForwardDirY)}: {WorldForwardDirY}, {nameof(WorldForwardDirZ)}: {WorldForwardDirZ}, " +
                $"{nameof(WorldRightDirX)}: {WorldRightDirX}, {nameof(WorldRightDirY)}: {WorldRightDirY}, {nameof(WorldRightDirZ)}: {WorldRightDirZ}, " +
                $"{nameof(GForceLateral)}: {GForceLateral}, {nameof(GForceLongitudinal)}: {GForceLongitudinal}, {nameof(GForceVertical)}: {GForceVertical}, " +
                $"{nameof(Yaw)}: {Yaw}, {nameof(Pitch)}: {Pitch}, {nameof(Roll)}: {Roll}";
        }
    };
}

using System.Runtime.InteropServices;

namespace F1TelemetryNetCore.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct PacketHeader
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
}
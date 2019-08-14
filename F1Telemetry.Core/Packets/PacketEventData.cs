using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace F1TelemetryNetCore.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PacketEventData
    {
        private static Encoding Encoding = System.Text.Encoding.UTF8;

        public PacketHeader Header;               // Header

        private fixed byte EventStringCodeRaw[4]; // Event string code, see above
        public string EventStringCode => Encoding.GetString(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref EventStringCodeRaw[0]), 4));

        public override string ToString()
        {
            return $"{nameof(Header)}: {Header}, {nameof(EventStringCode)}: {EventStringCode}";
        }
    };
}

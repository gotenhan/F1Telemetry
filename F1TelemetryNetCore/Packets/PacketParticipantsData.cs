using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace F1TelemetryNetCore.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    unsafe struct PacketParticipantsData
    {
        public PacketHeader Header;            // Header

        public byte NumCars;           // Number of cars in the data

        private const int ParticipantsDataBufferSize = 20 * ParticipantData.Size;
        private fixed byte ParticipantsRaw[ParticipantsDataBufferSize];
        private Span<ParticipantData> Participants => new Span<ParticipantData>(Unsafe.AsPointer(ref ParticipantsRaw[0]), ParticipantsDataBufferSize);

        public override string ToString()
        {
            return $"{nameof(Header)}: {Header}, {nameof(NumCars)}: {NumCars}, {nameof(Participants)}: [{string.Join(";", Participants.ToArray())}]";
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    unsafe struct ParticipantData
    {
        private static readonly Encoding Utf8Encoding = Encoding.UTF8;

        public const int Size = 5 + 48;

        public byte AiControlled;           // Whether the vehicle is AI (1) or Human (0) controlled
        public byte DriverId;               // Driver id - see appendix
        public byte TeamId;                 // Team id - see appendix
        public byte RaceNumber;             // Race number of the car
        public byte Nationality;            // Nationality of the driver

        private fixed byte NameRaw[48];               // Name of participant in UTF-8 format – null terminated

        public string Name => Utf8Encoding.GetString(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref NameRaw[0]), 48));
        // Will be truncated with … (U+2026) if too long
        public override string ToString()
        {
            return $"{nameof(AiControlled)}: {AiControlled}, {nameof(DriverId)}: {DriverId}, {nameof(TeamId)}: {TeamId}, {nameof(RaceNumber)}: {RaceNumber}, {nameof(Nationality)}: {Nationality}, {nameof(Name)}: {Name}";
        }
    };
}

using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public sealed class TrackInfo : Event
    {
        public Track Track { get; }
        public ushort Length { get; }

        public TrackInfo(Track track, ushort length, ulong sessionId, DateTime timeStamp, float sessionTime) 
            :base(sessionId, timeStamp, sessionTime)
        {
            Track = track;
            Length = length;    
        }

        private bool Equals(TrackInfo other)
        {
            return Track == other.Track && Length == other.Length && SessionId == other.SessionId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is TrackInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SessionId.GetHashCode();
                hashCode = (hashCode * 397) ^ Track.GetHashCode();
                hashCode = (hashCode * 397) ^ Length.GetHashCode();
                return hashCode;
            }
        }
    }

    public enum Track: sbyte
    {
        Unknown = -1,
        Melbourne,
        PaulRicard,
        Shanghai,
        Sakhir,
        Catalunya,
        Monaco,
        Montreal,
        Silverstone,
        Hockenheim,
        Hungaroring,
        Spa,
        Monza,
        Singapore,
        Suzuka,
        AbuDhabi,
        Texas,
        Brazil,
        Austria,
        Sochi,
        Mexico,
        Baku,
        SakhirShort,
        SilverstoneShort,
        TexasShort,
        SuzukaShort
    }

}
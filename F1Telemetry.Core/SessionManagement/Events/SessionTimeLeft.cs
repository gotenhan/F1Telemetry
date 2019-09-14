using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public sealed class SessionTimeLeft : Event
    {
        public TimeSpan TimeLeft { get; }

        public SessionTimeLeft(TimeSpan timeLeft, ulong sessionId, DateTime timeStamp, float sessionTime)
            : base(sessionId, timeStamp, sessionTime)
        {
            TimeLeft = timeLeft;
        }

        public bool Equals(SessionTimeLeft other)
        {
            return TimeLeft.Equals(other.TimeLeft) && SessionId == other.SessionId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SessionTimeLeft other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = SessionId.GetHashCode();
            hashCode = (hashCode * 397) ^ TimeLeft.GetHashCode();
            return hashCode;
        }
    }
}
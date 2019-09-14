using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public sealed class SessionDuration : Event
    {
        public TimeSpan Duration { get; }

        public SessionDuration(TimeSpan duration, ulong sessionId, DateTime timeStamp, float sessionTime)
            : base(sessionId, timeStamp, sessionTime)
        {
            Duration = duration;
        }

        private bool Equals(SessionDuration other)
        {
            return Duration.Equals(other.Duration) && SessionId == other.SessionId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SessionDuration other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = SessionId.GetHashCode();
            hashCode = (hashCode * 397) ^ Duration.GetHashCode();
            return hashCode;
        }
    }
}

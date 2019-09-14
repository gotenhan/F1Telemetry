using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public sealed class SessionPause : SessionLifecycle
    {
        public SessionPause(ulong sessionId, DateTime timeStamp, float sessionTime)
            : base(sessionId, timeStamp, sessionTime)
        {
        }
        private bool Equals(SessionPause other)
        {
            return SessionId == other.SessionId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SessionPause other && Equals(other);
        }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }
    }
}

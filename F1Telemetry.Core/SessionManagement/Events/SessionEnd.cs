using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public sealed class SessionEnd : SessionLifecycle
    {
        public SessionEnd(ulong sessionId, DateTime timeStamp, float sessionTime)
            : base(sessionId, timeStamp, sessionTime)
        {
        }

        private bool Equals(SessionEnd other)
        {
            return SessionId == other.SessionId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SessionEnd other && Equals(other);
        }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }
    }
}

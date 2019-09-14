using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public sealed class SessionStart : SessionLifecycle
    {
        public SessionStart(ulong sessionId, DateTime timeStamp, float sessionTime)
            : base(sessionId, timeStamp, sessionTime)
        {
        }
        private bool Equals(SessionStart other)
        {
            return SessionId == other.SessionId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SessionStart other && Equals(other);
        }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }
    }
}
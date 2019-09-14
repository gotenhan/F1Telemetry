using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public sealed class SessionResume : SessionLifecycle
    {
        public SessionResume(ulong sessionId, DateTime timeStamp, float sessionTime)
            : base(sessionId, timeStamp, sessionTime)
        {
        }
        private bool Equals(SessionResume other)
        {
            return SessionId == other.SessionId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SessionResume other && Equals(other);
        }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }

    }
}

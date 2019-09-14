using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public abstract class SessionLifecycle: Event {
        protected SessionLifecycle(ulong sessionId, DateTime timeStamp, float sessionTime) : base(sessionId, timeStamp, sessionTime)
        {
        }
    }
}
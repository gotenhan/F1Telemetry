using System;
using System.Collections.Generic;
using System.Text;

namespace F1Telemetry.Core.Observables
{
    public class SessionEvent: Event
    {
        public enum EventTypes
        {
            Start,
            End
        }

        public EventTypes EventType { get; }

        public SessionEvent(EventTypes eventType, ulong sessionId, DateTime timeStamp, float sessionTime)
            : base(sessionId, timeStamp, sessionTime)
        {
            EventType = eventType;
        }
    }
}

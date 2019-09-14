using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public class Event
    {
        public DateTime TimeStamp { get; }
        public ulong SessionId { get; }
        public float SessionTime { get; }

        public Event(ulong sessionId, DateTime timeStamp, float sessionTime)
        {
            SessionId = sessionId;
            TimeStamp = timeStamp;
            SessionTime = sessionTime;
        }
    }
}
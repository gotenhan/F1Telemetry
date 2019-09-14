using System;
using System.Reactive.Linq;
using F1Telemetry.Core.SessionManagement.Events;
using F1Telemetry.Core.Utils;

namespace F1Telemetry.Core.SessionManagement
{
    public sealed class Session
    {
        public ulong SessionId { get; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public bool Paused { get; private set; }
        public bool Completed { get; private set; }
        public IObservable<SessionEnd> SessionEnded { get; private set; }

        public IObservable<WeatherInfo> WeatherEvents { get; }
        public IObservable<SessionTimeLeft> TimeLeftEvents { get; }
        public IObservable<TrackInfo> TrackInfoEvents { get; }
        public IObservable<SessionDuration> DurationEvents { get; }
        public IObservable<SessionLifecycle> LifecycleEvents { get; }

        public Session(ulong sessionId, IObservable<Event> sessionEvents)
        {
            SessionId = sessionId;

            WeatherEvents = sessionEvents.OfType<WeatherInfo>().DistinctUntilChanged().Replay(1).With(o => o.Connect());
            TimeLeftEvents = sessionEvents.OfType<SessionTimeLeft>().DistinctUntilChanged().Replay(1).With(o => o.Connect());
            TrackInfoEvents = sessionEvents.OfType<TrackInfo>().DistinctUntilChanged().Replay(1).With(o => o.Connect());
            DurationEvents = sessionEvents.OfType<SessionDuration>().DistinctUntilChanged().Replay(1).With(o => o.Connect());
            var sessionStarts = sessionEvents.OfType<SessionStart>().DistinctUntilChanged().Replay(1).With(o => o.Connect()).Cast<SessionLifecycle>();
            var sessionEnds = sessionEvents.OfType<SessionEnd>().DistinctUntilChanged().Replay(1).With(o => o.Connect()).Cast<SessionLifecycle>();
            var sessionPauseResumes = sessionEvents.Where(e => e is SessionPause || e is SessionResume).DistinctUntilChanged().Replay(1).With(o => o.Connect()).Cast<SessionLifecycle>();
            LifecycleEvents = Observable.Merge(sessionStarts, sessionPauseResumes, sessionEnds);

            LifecycleEvents.Subscribe(HandleLifecycle, () => Completed = true);
            SessionEnded = sessionEvents.OfType<SessionEnd>();
        }
        
        private void HandleLifecycle(SessionLifecycle le)
        {
            switch (le)
            {
                case SessionStart ss: StartTime = ss.TimeStamp; break;
                case SessionEnd se: EndTime = se.TimeStamp; break;
                case SessionPause _: Paused = true; break;
                case SessionResume _: Paused = false; break;
                default: throw new InvalidOperationException($"Unsupported lifecycle {le?.GetType()}");
            };
        }
    }
}

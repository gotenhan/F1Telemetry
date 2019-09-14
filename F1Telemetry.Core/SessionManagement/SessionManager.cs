using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using F1Telemetry.Core.SessionManagement.Events;

namespace F1Telemetry.Core.SessionManagement
{
    public interface ISessionManager
    {
        IObservable<Session> Sessions { get; }
        int SessionCount { get; }
    }

    public sealed class SessionManager : ISessionManager
    {
        private readonly Dictionary<ulong, Session> _sessions;

        public IObservable<Session> Sessions { get; }

        public int SessionCount => _sessions.Count;

        public SessionManager(IEventPublisher eventPublisher)
        {
            _sessions = new Dictionary<ulong, Session>();
            var sessions = eventPublisher.Events.
                ObserveOn(new EventLoopScheduler(ts => new Thread(ts) { Name = "SessionManager event loop scheduler", IsBackground = true })).
                GroupByUntil(e => e.SessionId, g => g.OfType<SessionEnd>()).
                Select(g => new Session(g.Key, g)).
                Do(s => _sessions[s.SessionId] = s).
                Replay();
            Sessions = sessions;
            sessions.Connect();
        }
    }
}

using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using F1Telemetry.Core.SessionManagement;
using F1Telemetry.Core.SessionManagement.Events;
using F1TelemetryNetCore.Annotations;

namespace F1TelemetryNetCore
{
    class SessionViewModel: INotifyPropertyChanged
    {
        private DateTime _sessionStarted;
        private DateTime _sessionEnded;
        private bool _paused;
        private SessionTimerViewModel _timer;
        private WeatherViewModel _weather;
        private TrackViewModel _track;

        public ulong SessionId { get; }

        public DateTime SessionStarted
        {
            get => _sessionStarted;
            private set
            {
                if (value.Equals(_sessionStarted)) return;
                _sessionStarted = value;
                OnPropertyChanged();
            }
        }

        public DateTime SessionEnded
        {
            get => _sessionEnded;
            private set
            {
                if (value.Equals(_sessionEnded)) return;
                _sessionEnded = value;
                OnPropertyChanged();
            }
        }

        public bool Paused
        {
            get => _paused;
            set
            {
                if (value == _paused) return;
                _paused = value;
                OnPropertyChanged();
            }
        }

        public SessionTimerViewModel Timer
        {
            get => _timer;
            set
            {
                if (Equals(value, _timer)) return;
                _timer = value;
                OnPropertyChanged();
            }
        }

        public WeatherViewModel Weather
        {
            get => _weather;
            set
            {
                if (Equals(value, _weather)) return;
                _weather = value;
                OnPropertyChanged();
            }
        }

        public TrackViewModel Track
        {
            get => _track;
            set
            {
                if (Equals(value, _track)) return;
                _track = value;
                OnPropertyChanged();
            }
        }

        public SessionViewModel(Session session)
        {
            SessionId = session.SessionId;
            session.LifecycleEvents.ObserveOn(SynchronizationContext.Current).Subscribe(le =>
            {
                switch (le)
                {
                    case SessionStart ss:
                        SessionStarted = ss.TimeStamp;
                        break;
                    case SessionEnd se:
                        SessionEnded = SessionStarted.AddSeconds(le.SessionTime);
                        break;
                    case SessionPause _: Paused = true; break;
                    case SessionResume _: Paused = false; break;
                }
            });
            Timer = new SessionTimerViewModel(session.TimeLeftEvents, session.DurationEvents);
            Weather = new WeatherViewModel(session.WeatherEvents);
            Track = new TrackViewModel(session.TrackInfoEvents);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Equals(SessionViewModel other)
        {
            return SessionId == other.SessionId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((SessionViewModel) obj);
        }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }
    }
}

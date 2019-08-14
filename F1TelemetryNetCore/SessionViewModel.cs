using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using F1Telemetry.Core.Observables;
using F1TelemetryNetCore.Annotations;

namespace F1TelemetryNetCore
{
    class SessionViewModel: INotifyPropertyChanged
    {
        private DateTime _sessionStarted;
        private DateTime _sessionEnded;
        private SessionState _state;
        private TrackInfo _track;
        private WeatherInfo _currentWeather;

        public ulong SessionId { get; }

        public WeatherInfo CurrentWeather
        {
            get => _currentWeather;
            private set
            {
                if (Equals(value, _currentWeather)) return;
                _currentWeather = value;
                OnPropertyChanged();
            }
        }

        public TrackInfo Track
        {
            get => _track;
            private set
            {
                if (Equals(value, _track)) return;
                _track = value;
                OnPropertyChanged();
            }
        }

        public SessionState State
        {
            get => _state;
            private set
            {
                if (Equals(value, _state)) return;
                _state = value;
                OnPropertyChanged();
            }
        }

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

        public SessionViewModel(ulong sessionId, IObservable<Event> sessionEvents)
        {
            SessionId = sessionId;
            var subscription = sessionEvents.Subscribe(e =>
            {
                switch (e)
                {
                    case SessionEvent se:
                    {
                        if (se.EventType == SessionEvent.EventTypes.Start)
                        {
                            SessionStarted = se.TimeStamp;
                        }
                        else if (se.EventType == SessionEvent.EventTypes.End)
                        {
                            SessionEnded = SessionStarted.AddSeconds(se.SessionTime);
                        }
                        break;
                    }
                    case Session s:
                    {
                        CurrentWeather = s.Weather;
                        Track = s.Track;
                        State = s.State;
                        break;
                    }

                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

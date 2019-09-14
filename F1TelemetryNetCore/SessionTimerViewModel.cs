using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using F1Telemetry.Core.SessionManagement.Events;
using F1TelemetryNetCore.Annotations;

namespace F1TelemetryNetCore
{
    class SessionTimerViewModel: INotifyPropertyChanged
    {
        private TimeSpan _timeLeft;
        private TimeSpan _duration;

        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            set
            {
                if (value.Equals(_timeLeft)) return;
                _timeLeft = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value.Equals(_duration)) return;
                _duration = value;
                OnPropertyChanged();
            }
        }

        public SessionTimerViewModel(IObservable<SessionTimeLeft> timeLeft, IObservable<SessionDuration> sessionDuration)
        {
            sessionDuration.ObserveOn(SynchronizationContext.Current).Subscribe(d => Duration = d.Duration);
            timeLeft.ObserveOn(SynchronizationContext.Current).Subscribe(tl => TimeLeft = tl.TimeLeft);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

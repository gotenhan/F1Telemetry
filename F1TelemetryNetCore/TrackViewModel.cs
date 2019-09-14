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
    class TrackViewModel:INotifyPropertyChanged
    {
        private string _name;
        private int _length;

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public int Length
        {
            get => _length;
            set
            {
                if (value == _length) return;
                _length = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TrackViewModel(IObservable<TrackInfo> trackUpdates)
        {
            trackUpdates.ObserveOn(SynchronizationContext.Current).Subscribe(tu =>
            {
                Name = tu.Track.ToString();
                Length = tu.Length;
            });
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

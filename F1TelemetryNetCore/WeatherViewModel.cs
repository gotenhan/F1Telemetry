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
    class WeatherViewModel: INotifyPropertyChanged
    {
        private int _airTemperature;
        private int _trackTemperature;
        private Weather _weather;

        public int AirTemperature
        {
            get => _airTemperature;
            set
            {
                if (value == _airTemperature) return;
                _airTemperature = value;
                OnPropertyChanged();
            }
        }

        public int TrackTemperature
        {
            get => _trackTemperature;
            set
            {
                if (value == _trackTemperature) return;
                _trackTemperature = value;
                OnPropertyChanged();
            }
        }

        public Weather Weather
        {
            get => _weather;
            set
            {
                if (value == _weather) return;
                _weather = value;
                OnPropertyChanged();
            }
        }

        public WeatherViewModel(IObservable<WeatherInfo> weatherUpdates)
        {
            weatherUpdates.ObserveOn(SynchronizationContext.Current).Subscribe(we =>
            {
                AirTemperature = we.AirTemperature;
                TrackTemperature = we.TrackTemperature;
                Weather = we.Weather;
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

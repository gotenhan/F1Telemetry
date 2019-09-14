using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using F1TelemetryNetCore.Annotations;

namespace F1TelemetryNetCore
{
    public class RaceStandingsViewModel
    {
        public ObservableCollection<DriverPositionViewModel> Drivers { get; } = new ObservableCollection<DriverPositionViewModel>();
        public ICollectionView Standings { get; }

        public RaceStandingsViewModel()
        {
            var driversViewSource = new CollectionViewSource()
            {
                Source = Drivers
            };
            Standings = driversViewSource.View;
        }

        public class DriverPositionViewModel: INotifyPropertyChanged
        {
            public int Position { get; set; }
            public string Name { get; set; }
            public TimeSpan TotalTime { get; set; }
            public TimeSpan LapTime { get; set; }
            public TimeSpan FastestLap { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

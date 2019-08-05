using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using F1TelemetryNetCore.Annotations;
using Prism.Commands;

namespace F1TelemetryNetCore
{
    class F1TelemetryViewModel: INotifyPropertyChanged
    {
        public F1TelemetryViewModel()
        {
            Port = 20777;
            StartListeningCommand = new DelegateCommand(() => _telemetryListener.StartListening(Port));
            StopListeningCommand = new DelegateCommand(() => _telemetryListener.StopListening());
            _telemetryListener = new TelemetryListener();
            LogEntries = new ObservableCollection<LogEntry>();
        }

        private TelemetryListener _telemetryListener;
        private int _port;

        public int Port
        {
            get => _port;
            set
            {
                if (value == _port) return;
                _port = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LogEntry> LogEntries { get; set; }

        public ICommand StartListeningCommand { get; set; }
        public ICommand StopListeningCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

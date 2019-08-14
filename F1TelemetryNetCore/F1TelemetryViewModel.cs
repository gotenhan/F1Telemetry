using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Pipelines;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using F1Telemetry.Core;
using F1Telemetry.Core.Observables;
using F1TelemetryNetCore.Annotations;
using log4net;
using log4net.Repository.Hierarchy;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace F1TelemetryNetCore
{
    class F1TelemetryViewModel: INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private UdpListener _udpListener;
        private PacketParser _parser;
        private CancellationTokenSource _fileParsingCts;

        private int _port;
        private Pipe _pipe;
        private readonly OpenFileDialog _dialog;
        private string _filePath;
        private bool _isReading;

        public F1TelemetryViewModel()
        {
            Port = 20777;
            FilePath = @"C:\Users\adrian\Moje projekty\F1Telemetry\TestData\Australia3Laps.rawbytes";

            StartListeningCommand = new DelegateCommand(StartListening, CanStartListening);
            StopListeningCommand = new DelegateCommand(StopListening, CanStopListening);
            SelectFileCommand = new DelegateCommand(SelectFile);
            ReadFromFileCommand = new DelegateCommand(ReadFromFile, () => !IsReading).ObservesProperty(() => IsReading);
            StopReadingCommand = new DelegateCommand(ReadFromFile, () => IsReading).ObservesProperty(() => IsReading);

            Sessions = new ObservableCollection<SessionViewModel>();

            var packetHandler = new PacketHandlerObservable();
            packetHandler.Events
                .ObserveOn(SynchronizationContext.Current)
                .GroupBy(e => e.SessionId)
                .Select(sessionEvents => new SessionViewModel(sessionEvents.Key, sessionEvents))
                .Subscribe(svm => Sessions.Add(svm));

            _parser = new PacketParser(packetHandler);
            _dialog = new OpenFileDialog();
            _dialog.Multiselect = false;
            _dialog.CheckFileExists = true;
            _dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _dialog.RestoreDirectory = true;
        }

        #region Listening on udp

        public DelegateCommand StartListeningCommand { get; set; }
        public DelegateCommand StopListeningCommand { get; set; }

        public ObservableCollection<SessionViewModel> Sessions { get; }

        public int Port
        {
            get => _port;
            set
            {
                if (value == _port) return;
                _port = value;
                OnPropertyChanged();
                StartListeningCommand?.RaiseCanExecuteChanged();
                StopListeningCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool CanStopListening() => _udpListener?.Listening ?? false;

        private void StopListening() => _udpListener?.StopListening();

        private bool CanStartListening() => _udpListener == null || !(_udpListener.Listening && _udpListener.Port == _port);

        private void StartListening()
        {
            if (_udpListener == null || _udpListener.Listening == false)
            {
                _udpListener = new UdpListener(Port);
                _pipe = new Pipe();
                _udpListener.StartListening(_pipe.Writer);
                _udpListener.OnStopListening += OnListenerStopped;

                Task.Factory.StartNew(async () => await _parser.ReadMessages(_pipe.Reader, CancellationToken.None),
                    TaskCreationOptions.LongRunning).Unwrap().ContinueWith(
                    (t, _) => { Logger.Error("Caught reader error in VM", t.Exception); }
                    ,CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted);

                StartListeningCommand?.RaiseCanExecuteChanged();
                StopListeningCommand?.RaiseCanExecuteChanged();
            }
        }

        private void OnListenerStopped(object sender, Exception e)
        {
            var listener = (UdpListener) sender;
            if (listener == _udpListener)
            {
                StartListeningCommand?.RaiseCanExecuteChanged();
                StopListeningCommand?.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Reading from file

        public DelegateCommand SelectFileCommand { get; set; }
        public DelegateCommand ReadFromFileCommand { get; set; }
        public DelegateCommand StopReadingCommand { get; set; }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (value == _filePath) return;
                _filePath = value;
                OnPropertyChanged();
            }
        }

        private void SelectFile()
        {
            if (_dialog.ShowDialog() == true)
            {
                FilePath = _dialog.FileName;
            }
        }

        public bool IsReading
        {
            get => _isReading;
            private set
            {
                if (value == _isReading) return;
                _isReading = value;
                OnPropertyChanged();
            }
        }

        private async void ReadFromFile()
        {
            var path = FilePath;
            if (File.Exists(path))
            {
                IsReading = true;
                _fileParsingCts = new CancellationTokenSource();
                try
                {
                    var pipe = new Pipe();
                    var parserTask = _parser.ReadMessages(pipe.Reader, _fileParsingCts.Token);
                    var fileReaderTask = FileReader.ReadFromFile(path, pipe.Writer, _fileParsingCts.Token);
                    await parserTask;
                    await fileReaderTask;
                }
                finally
                {
                    IsReading = false;
                }
            }
        }

        private void StopReading() => _fileParsingCts.Cancel();

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}

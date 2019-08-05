using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using F1TelemetryNetCore.Annotations;
using log4net.Appender;
using log4net.Core;

namespace F1TelemetryNetCore
{
    public class WpfLogAppender: AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            var msg = RenderLoggingEvent(loggingEvent);
            var logEntry = new LogEntry()
            {
                DateTime = loggingEvent.TimeStampUtc,
                Message = msg
            };
        }
    }

    public class LogEntry : INotifyPropertyChanged
    {
        public DateTime DateTime { get; set; }

        public string Message { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

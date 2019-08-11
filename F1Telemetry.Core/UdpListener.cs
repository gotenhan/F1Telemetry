using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace F1Telemetry.Core
{
    public class UdpListener: IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private volatile bool _listening;
        private CancellationTokenSource _cts;
        private Timer _timer;

        public UdpListener(int port)
        {
            Port = port;
        }

        public bool Listening => _listening;
        public int Port { get; }

        public event EventHandler<Exception> OnStopListening;

        public void StartListening(PipeWriter writer)
        {
            if (!_listening)
            {
                Logger.Info("Start listening");
                _cts = new CancellationTokenSource();
                var networkThread = new Thread(DoStartListening);
                networkThread.Name = "Network thread";

                _listening = true;
                networkThread.Start((writer, _cts.Token));
            }
            else
            {
                Logger.Info("Already listening");
                throw new InvalidOperationException($"Already listening on port {Port}");
            }
        }

        public void StopListening()
        {
            if (!_listening)
            {
                return;
            }

            Logger.Info("Stopping listening");
            _listening = false;
            _cts?.Cancel();
        }

        private async void DoStartListening(object o)
        {
            PipeWriter writer = null;
            CancellationToken ct;
            UdpClient udpClient = null;
            Exception exception = null;
            try
            {
                (writer, ct) = (ValueTuple<PipeWriter, CancellationToken>) o;
                writer.OnReaderCompleted((ex, oo) =>
                {
                    exception = ex;
                    _listening = false;
                }, null);

                using (udpClient = new UdpClient(Port))
                {
                    _timer = new Timer(uc =>
                    {
                        if (!_listening || ct.IsCancellationRequested)
                        {
                            ((UdpClient)uc)?.Close();
                            _timer?.Dispose();
                        }

                    }, udpClient, 1000, 1000);

                    while (_listening && !ct.IsCancellationRequested)
                    {
                        var result = await udpClient.ReceiveAsync();
                        var buffer = result.Buffer;

                        if (buffer.Length == 0)
                            continue;

                        if (!PacketParser.ValidatePacketHeaderAndLength(buffer))
                        {
                            continue;
                        }

                        
                        await writer.WriteAsync(buffer, _cts.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occured while receiving data from network", ex);
                exception = ex;
            }
            finally
            {
                if (exception != null)
                {
                    Logger.Warn("Finished writing because reader completed with exception", exception);
                }

                _timer?.Dispose();
                writer?.Complete();
                udpClient?.Close();
                _listening = false;
            }

            OnStopListening?.Invoke(this, exception);
        }

        public void Dispose()
        {
            _listening = false;
        }
    }
}

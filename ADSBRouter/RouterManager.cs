using ADSBRouter;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Feed.ADSBRouter
{
    public class RouterManager
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<RouterManager> _logger;
        public TcpInput Input { get; set; }
        public List<TcpOutput> Outputs { get; set; }

        public RouterManager(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<RouterManager>();
            Input = new(loggerFactory.CreateLogger<TcpInput>());
            Outputs = new();
        }

        public async void AddOutputAsync(string hostname, int port, CancellationToken stoppingToken)
        {
            var output = new TcpOutput(_loggerFactory.CreateLogger<TcpOutput>(), hostname, port);
            _logger.LogInformation($"Added Output {output.Hostname}");
            _ = output.ConnectAsync(stoppingToken);
            Outputs.Add(output);
        }

        public void RemoveOutputAt(int index)
        {
            if (index > Outputs.Count)
            {
                return;
            }

            Outputs.RemoveAt(index);
        }

        public void RemoteOutputByHostname(string hostname)
        {
            var output = Outputs.Find(m => m.Hostname == hostname);

            if (output is null)
            {
                return;
            }

            Outputs.Remove(output);
        }

        public void Listen(IPAddress address, int port)
        {
            Input.EndPoint = new IPEndPoint(address, port);
            Input.Start();
        }

        public void StopListening()
        {
            Input.Stop();
        }

        public async Task HandleAsync(CancellationToken stoppingToken)
        {
            try
            {
                var client = await Input.AcceptClient(stoppingToken);
                await HandleClientAsync(client, stoppingToken);
            }
            catch (OperationCanceledException) { }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation($"New client connected {client.Client.RemoteEndPoint}");
                var stream = client.GetStream();

                while (client.Connected)
                {
                    Outputs.ForEach(output =>
                    {
                        if (!output.Connected)
                        {
                            _ = output.ConnectAsync(stoppingToken);
                        }
                    });

                    byte[] buffer = new byte[2048];
                    int length = await stream.ReadAsync(buffer, stoppingToken);

                    Outputs.ForEach(async output => await output.WriteAsync(buffer, length, stoppingToken));
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
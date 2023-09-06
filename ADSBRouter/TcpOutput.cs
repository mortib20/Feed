using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ADSBRouter
{
    public class TcpOutput
    {
        private readonly ILogger<TcpOutput> _logger;
        public bool Reconnect { get; set; } = false;
        public TcpClient Client { get; private set; }

        public string EndPoint { get; }

        public bool Connected { get => Client.Connected; }
        public string Hostname { get; }
        private int Port { get; }

        public TcpOutput(ILogger<TcpOutput> logger, string hostname, int port)
        {
            _logger = logger;
            EndPoint = $"{hostname}:{port}";
            Hostname = hostname;
            Port = port;
            Client = new();
        }

        public async Task ConnectAsync(CancellationToken stoppingToken)
        {
            if (Reconnect) return;

            while (!Client.Connected && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Reconnect = true;
                    Client = new TcpClient();
                    await Client.ConnectAsync(Hostname, Port, stoppingToken);
                    _logger.LogInformation($"{EndPoint} Connected");
                    break;
                }
                catch (OperationCanceledException) { }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.HostNotFound)
                    {
                        _logger.LogInformation($"{EndPoint} No Hostname found - trying again a bit later");
                        await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation($"{EndPoint} Socket failed - trying again - {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"{EndPoint} failed - trying again - {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            Reconnect = false;
        }

        public async Task WriteAsync(byte[] buffer, int length, CancellationToken stoppingToken)
        {
            if (Reconnect) return;

            try
            {
                if (!Client.Connected)
                {
                    await ConnectAsync(stoppingToken);
                }

                var stream = Client.GetStream();
                await stream.WriteAsync(buffer, 0, length, stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogInformation($"{EndPoint} Write failed - {ex.Message}");
            }
        }
    }
}

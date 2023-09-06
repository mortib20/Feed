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
    public class TcpInput
    {
        private readonly ILogger<TcpInput> _logger;
        private TcpListener? _listener;

        public IPEndPoint EndPoint
        {
            get
            {
                return (IPEndPoint)_listener.LocalEndpoint;
            }
            set
            {
                _listener = new(value);
            }
        }

        public TcpInput(ILogger<TcpInput> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            if (_listener is null)
            {
                throw new NullReferenceException("Listener not initialized");
            }

            _logger.LogInformation($"Listening on {EndPoint}");
            _listener.Start();
        }

        public void Stop()
        {
            if (_listener is null)
            {
                throw new NullReferenceException("Listener not initialized");
            }

            _logger.LogInformation($"Listening on {EndPoint} stopped");
            _listener.Stop();
        }

        public async Task<TcpClient> AcceptClient(CancellationToken stoppingToken)
        {
            return await _listener.AcceptTcpClientAsync(stoppingToken);
        }
    }
}

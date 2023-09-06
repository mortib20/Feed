using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Feed.ADSBRouter
{
    public class RouterManagerService : BackgroundService
    {
        private readonly ILogger<RouterManagerService> _logger;
        private readonly RouterManager _router;

        public RouterManagerService(ILogger<RouterManagerService> logger, RouterManager routerManager)
        {
            _logger = logger;
            _router = routerManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting RouterManager Service");

                _router.AddOutputAsync("feed.adsb.lol", 30004, stoppingToken);
                //_router.AddOutputAsync("192.168.168.3", 3278, stoppingToken);

                _router.Listen(IPAddress.Any, 30004);

                await _router.HandleAsync(stoppingToken);

                _router.StopListening();
                _logger.LogInformation("Stopped RouterManager Service");

            }
            catch (OperationCanceledException) { }
        }
    }
}
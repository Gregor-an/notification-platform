using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orchestrator.Services;

namespace Orchestrator.Workers
{
    public sealed class NotificationProcessingWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationProcessingWorker> _logger;

        public NotificationProcessingWorker(IServiceScopeFactory _scopeFactory, 
                ILogger<NotificationProcessingWorker> logger)
        {
            this._scopeFactory = _scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider
                        .GetRequiredService<NotificationProcessingService>();

                    await processor.ProcessPendingAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in NotificationProcessingWorker.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
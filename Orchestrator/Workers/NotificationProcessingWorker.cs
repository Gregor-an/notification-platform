using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Orchestrator.Workers
{
    public sealed class NotificationProcessingWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationProcessingWorker> _logger;

        public NotificationProcessingWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationProcessingWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    var senders = scope.ServiceProvider.GetServices<INotificationSender>().ToList();

                    var notifications = await repository.GetPendingBatchAsync(10, stoppingToken);

                    foreach (var notification in notifications)
                    {
                        notification.MarkProcessing();
                        var attempt = notification.StartAttempt();

                        var sender = senders.FirstOrDefault(x => x.ChannelType == notification.ChannelType);

                        if (sender is null)
                        {
                            attempt.MarkFailed($"No sender configured for channel {notification.ChannelType}.");
                            notification.MarkFailed();
                            continue;
                        }

                        var result = await sender.SendAsync(notification, stoppingToken);

                        if (result.IsSuccess)
                        {
                            attempt.MarkSucceeded();
                            notification.MarkDelivered();
                        }
                        else
                        {
                            attempt.MarkFailed(result.ErrorMessage ?? "Unknown error.");
                            notification.MarkFailed();
                        }
                    }

                    await repository.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during notification processing.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}

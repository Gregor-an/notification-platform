using Application.Interfaces.Providers;
using Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Orchestrator.Services
{
    public sealed class NotificationProcessingService
    {
        private readonly INotificationRepository _repository;
        private readonly IReadOnlyList<INotificationSender> _senders;
        private readonly ILogger<NotificationProcessingService> _logger;

        public NotificationProcessingService(INotificationRepository repository, IEnumerable<INotificationSender> senders, ILogger<NotificationProcessingService> logger)
        {
            _repository = repository;
            _senders = senders.ToList();
            _logger = logger;
        }

        public async Task ProcessPendingAsync(CancellationToken cancellationToken)
        {
            var notifications = await _repository.GetPendingBatchAsync(10, cancellationToken);

            foreach (var notification in notifications)
            {
                try
                {
                    notification.MarkProcessing();
                    var attempt = notification.StartAttempt();

                    var sender = _senders.FirstOrDefault(x => x.ChannelType == notification.ChannelType);

                    if (sender is null)
                    {
                        attempt.MarkFailed($"No sender configured for channel {notification.ChannelType}.");
                        notification.MarkFailed();

                        _logger.LogWarning("No sender for channel {Channel}. NotificationId: {Id}", notification.ChannelType, notification.Id);

                        await _repository.SaveChangesAsync(cancellationToken);
                        continue;
                    }

                    var result = await sender.SendAsync(notification, cancellationToken);

                    if (result.IsSuccess)
                    {
                        attempt.MarkSucceeded();
                        notification.MarkDelivered();

                        _logger.LogInformation( "Notification {Id} delivered successfully.", notification.Id);
                    }
                    else
                    {
                        attempt.MarkFailed(result.ErrorMessage ?? "Unknown error.");
                        notification.MarkFailed();

                        _logger.LogWarning("Notification {Id} failed: {Error}", notification.Id, result.ErrorMessage);
                    }

                    await _repository.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    try
                    {
                        notification.MarkFailed();
                        await _repository.SaveChangesAsync(cancellationToken);
                    }
                    catch{}

                    _logger.LogError(ex,"Unexpected error processing notification {Id}.", notification.Id);
                }
            }
        }
    }
}
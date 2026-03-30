using Application.Interfaces.Providers;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchestrator.Settings;

namespace Orchestrator.Services
{
    public sealed class NotificationProcessingService
    {
        private readonly INotificationRepository _repository;
        private readonly IReadOnlyList<INotificationSender> _senders;
        private readonly ILogger<NotificationProcessingService> _logger;
        private readonly NotificationProcessingOptions _options;

        public NotificationProcessingService(INotificationRepository repository,
            IEnumerable<INotificationSender> senders, ILogger<NotificationProcessingService> logger,
            IOptions<NotificationProcessingOptions> options)
        {
            _repository = repository;
            _senders = senders.ToList();
            _logger = logger;
            _options = options.Value;
        }

        public async Task ProcessPendingAsync(CancellationToken cancellationToken)
        {
            var pending = await _repository.GetPendingBatchAsync(_options.BatchSize, cancellationToken);
            foreach (var notification in pending)
                await ProcessNotificationAsync(notification, cancellationToken);

            var retryable = await _repository.GetRetryBatchAsync(_options.BatchSize, _options.MaxAttempts, cancellationToken);
            foreach (var notification in retryable)
                await ProcessNotificationAsync(notification, cancellationToken);
        }

        private async Task ProcessNotificationAsync(Notification notification, CancellationToken cancellationToken)
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
                    return;
                }

                var result = await sender.SendAsync(notification, cancellationToken);

                if (result.IsSuccess)
                {
                    attempt.MarkSucceeded();
                    notification.MarkDelivered();

                    _logger.LogInformation("Notification {Id} delivered successfully.", notification.Id);
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
                catch { }

                _logger.LogError(ex, "Unexpected error processing notification {Id}.", notification.Id);
            }
        }
    }
}

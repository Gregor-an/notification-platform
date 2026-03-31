using Application.DTOs;
using Application.Interfaces.Repositories;

namespace Application.Notifications.Queries.GetNotificationById
{
    public sealed class GetNotificationByIdQueryHandler
    {
        private readonly INotificationRepository _repository;

        public GetNotificationByIdQueryHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<NotificationDetailDto?> HandleAsync(
            GetNotificationByIdQuery query,
            CancellationToken cancellationToken)
        {
            var notification = await _repository.GetByIdAsync(query.Id, cancellationToken);

            if (notification is null)
                return null;

            return new NotificationDetailDto
            {
                Id = notification.Id,
                Recipient = notification.Recipient.Value,
                Subject = notification.Content.Subject,
                Body = notification.Content.Body,
                ChannelType = notification.ChannelType,
                Status = notification.Status,
                Priority = notification.Priority,
                CreatedUtc = notification.CreatedUtc,
                ProcessedUtc = notification.ProcessedUtc,
                Attempts = notification.Attempts.Select(a => new DeliveryAttemptDto
                {
                    AttemptNumber = a.AttemptNumber,
                    Status = a.Status,
                    FailureReason = a.FailureReason,
                    CreatedUtc = a.CreatedUtc,
                    CompletedUtc = a.CompletedUtc
                }).ToList()
            };
        }
    }
}

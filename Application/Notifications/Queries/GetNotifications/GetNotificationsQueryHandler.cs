using Application.DTOs;
using Application.Interfaces.Repositories;

namespace Application.Notifications.Queries.GetNotifications
{
    public sealed class GetNotificationsQueryHandler
    {
        private readonly INotificationRepository _repository;

        public GetNotificationsQueryHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<(List<NotificationSummaryDto> Items, int TotalCount)> HandleAsync(
            GetNotificationsQuery query,
            CancellationToken cancellationToken)
        {
            var (notifications, totalCount) = await _repository.GetPagedAsync(
                query.Page, query.PageSize, cancellationToken);

            var items = notifications.Select(n => new NotificationSummaryDto
            {
                Id = n.Id,
                Recipient = n.Recipient.Value,
                ChannelType = n.ChannelType,
                Status = n.Status,
                Priority = n.Priority,
                CreatedUtc = n.CreatedUtc,
                AttemptCount = n.Attempts.Count
            }).ToList();

            return (items, totalCount);
        }
    }
}

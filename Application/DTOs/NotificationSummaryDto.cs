using Domain.Enums;

namespace Application.DTOs
{
    public sealed class NotificationSummaryDto
    {
        public Guid Id { get; init; }
        public string Recipient { get; init; } = string.Empty;
        public ChannelType ChannelType { get; init; }
        public NotificationStatus Status { get; init; }
        public NotificationPriority Priority { get; init; }
        public DateTime CreatedUtc { get; init; }
        public int AttemptCount { get; init; }
    }
}

using Domain.Enums;

namespace Application.DTOs
{
    public sealed class NotificationDetailDto
    {
        public Guid Id { get; init; }
        public string Recipient { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public ChannelType ChannelType { get; init; }
        public NotificationStatus Status { get; init; }
        public NotificationPriority Priority { get; init; }
        public DateTime CreatedUtc { get; init; }
        public DateTime? ProcessedUtc { get; init; }
        public List<DeliveryAttemptDto> Attempts { get; init; } = [];
    }
}

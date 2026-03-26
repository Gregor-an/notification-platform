using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Notification
    {
        private readonly List<DeliveryAttempt> _attempts = new();

        public Guid Id { get; private set; }
        public Recipient Recipient { get; private set; } = null!;
        public MessageContent Content { get; private set; } = null!;
        public ChannelType ChannelType { get; private set; }
        public NotificationPriority Priority { get; private set; }
        public NotificationStatus Status { get; private set; }
        public DateTime CreatedUtc { get; private set; }
        public DateTime? ProcessedUtc { get; private set; }

        public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

        private Notification() { }

        public Notification(Recipient recipient, MessageContent content, 
                            ChannelType channelType, NotificationPriority priority)
        {
            Id = Guid.NewGuid();
            Recipient = recipient;
            Content = content;
            ChannelType = channelType;
            Priority = priority;
            Status = NotificationStatus.Pending;
            CreatedUtc = DateTime.UtcNow;
        }

        public void MarkProcessing()
        {
            if (Status != NotificationStatus.Pending && Status != NotificationStatus.Failed)
                throw new InvalidOperationException($"Notification in status {Status} cannot be moved to Processing.");

            Status = NotificationStatus.Processing;
        }

        public DeliveryAttempt StartAttempt()
        {
            if (Status != NotificationStatus.Processing)
                throw new InvalidOperationException("Attempt can only be started when notification is Processing.");

            var attempt = new DeliveryAttempt(Id, _attempts.Count + 1);
            _attempts.Add(attempt);

            return attempt;
        }

        public void MarkDelivered()
        {
            Status = NotificationStatus.Delivered;
            ProcessedUtc = DateTime.UtcNow;
        }

        public void MarkFailed()
        {
            Status = NotificationStatus.Failed;
            ProcessedUtc = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == NotificationStatus.Delivered)
                throw new InvalidOperationException("Delivered notification cannot be cancelled.");

            Status = NotificationStatus.Cancelled;
        }

        public bool CanRetry(int maxAttempts)
        {
            return Status == NotificationStatus.Failed && _attempts.Count < maxAttempts;
        }
    }
}

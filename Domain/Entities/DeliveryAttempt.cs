using Domain.Enums;

namespace Domain.Entities
{
    public class DeliveryAttempt
    {
        public Guid Id { get; private set; }
        public Guid NotificationId { get; private set; }
        public int AttemptNumber { get; private set; }
        public AttemptStatus Status { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTime CreatedUtc { get; private set; }
        public DateTime? CompletedUtc { get; private set; }

        private DeliveryAttempt() { }

        public DeliveryAttempt(Guid notificationId, int attemptNumber)
        {
            Id = Guid.NewGuid();
            NotificationId = notificationId;
            AttemptNumber = attemptNumber;
            Status = AttemptStatus.Pending;
            CreatedUtc = DateTime.UtcNow;
        }

        public void MarkSucceeded()
        {
            Status = AttemptStatus.Succeeded;
            CompletedUtc = DateTime.UtcNow;
            FailureReason = null;
        }

        public void MarkFailed(string reason)
        {
            Status = AttemptStatus.Failed;
            FailureReason = reason;
            CompletedUtc = DateTime.UtcNow;
        }
    }
}

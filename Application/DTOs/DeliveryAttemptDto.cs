using Domain.Enums;

namespace Application.DTOs
{
    public sealed class DeliveryAttemptDto
    {
        public int AttemptNumber { get; init; }
        public AttemptStatus Status { get; init; }
        public string? FailureReason { get; init; }
        public DateTime CreatedUtc { get; init; }
        public DateTime? CompletedUtc { get; init; }
    }
}

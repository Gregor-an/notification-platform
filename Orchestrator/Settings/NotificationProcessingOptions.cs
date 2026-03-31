namespace Orchestrator.Settings
{
    public sealed class NotificationProcessingOptions
    {
        public const string SectionName = "NotificationProcessing";

        public int BatchSize { get; init; } = 10;
        public int IntervalSeconds { get; init; } = 5;
        public int MaxAttempts { get; init; } = 3;
    }
}

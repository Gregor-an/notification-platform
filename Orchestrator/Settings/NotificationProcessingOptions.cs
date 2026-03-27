using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchestrator.Settings
{
    public sealed class NotificationProcessingOptions
    {
        public const string SectionName = "NotificationProcessing";

        public int BatchSize { get; init; } = 10;
        public int IntervalSeconds { get; init; } = 5;
    }
}

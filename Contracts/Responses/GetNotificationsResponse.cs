using Application.DTOs;

namespace Contracts.Responses
{
    public sealed class GetNotificationsResponse
    {
        public List<NotificationSummaryDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

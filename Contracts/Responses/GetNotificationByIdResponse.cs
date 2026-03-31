using Application.DTOs;

namespace Contracts.Responses
{
    public sealed class GetNotificationByIdResponse
    {
        public NotificationDetailDto Notification { get; set; } = null!;
    }
}

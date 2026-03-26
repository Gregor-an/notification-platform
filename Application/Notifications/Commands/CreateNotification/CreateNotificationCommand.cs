using Domain.Enums;

namespace Application.Notifications.Commands.CreateNotification
{
    public sealed record CreateNotificationCommand(
        string Recipient,
        string Subject,
        string Body,
        ChannelType ChannelType,
        NotificationPriority Priority
    );
}
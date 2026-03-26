using Application.DTOs;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface INotificationSender
    {
        ChannelType ChannelType { get; }
        Task<SendResult> SendAsync(Notification notification, CancellationToken cancellationToken);
    }
}

using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Providers
{
    public sealed class MockEmailNotificationSender : INotificationSender
    {
        public ChannelType ChannelType => ChannelType.Email;

        public async Task<SendResult> SendAsync(Notification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(500, cancellationToken);

            var success = Random.Shared.Next(1, 101) > 20;

            return success
                ? SendResult.Success()
                : SendResult.Failure("Mock email provider failure.");
        }
    }
}

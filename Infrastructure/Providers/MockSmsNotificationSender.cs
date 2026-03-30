using Application.DTOs;
using Application.Interfaces.Providers;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Providers
{
    public sealed class MockSmsNotificationSender : INotificationSender
    {
        public ChannelType ChannelType => ChannelType.Sms;

        public async Task<SendResult> SendAsync(Notification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(300, cancellationToken);

            var success = Random.Shared.Next(1, 101) > 20;

            return success
                ? SendResult.Success()
                : SendResult.Failure("Mock SMS provider failure.");
        }
    }
}

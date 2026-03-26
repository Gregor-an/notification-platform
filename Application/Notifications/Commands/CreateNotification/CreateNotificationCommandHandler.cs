using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Notifications.Commands.CreateNotification
{
    public sealed class CreateNotificationCommandHandler
    {
        private readonly INotificationRepository _notificationRepository;

        public CreateNotificationCommandHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Guid> HandleAsync(CreateNotificationCommand command, CancellationToken cancellationToken)
        {
            var notification = new Notification(
                new Recipient(command.Recipient),
                new MessageContent(command.Subject, command.Body),
                command.ChannelType,
                command.Priority
            );
            
            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _notificationRepository.SaveChangesAsync(cancellationToken);

            return notification.Id;
        }
    }
}
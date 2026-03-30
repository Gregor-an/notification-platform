using Application.Interfaces.Repositories;
using Application.Notifications.Queries.GetNotificationById;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace UnitTests.Application
{
    public sealed class GetNotificationByIdQueryHandlerTests
    {
        private readonly Mock<INotificationRepository> _repositoryMock = new();
        private readonly GetNotificationByIdQueryHandler _handler;

        public GetNotificationByIdQueryHandlerTests()
        {
            _handler = new GetNotificationByIdQueryHandler(_repositoryMock.Object);
        }

        private static Notification CreateNotification() => new(
            new Recipient("user@example.com"),
            new MessageContent("Hello", "Body text"),
            ChannelType.Email,
            NotificationPriority.Normal);

        [Fact]
        public async Task HandleAsync_WhenNotificationExists_ShouldReturnDetailDto()
        {
            var notification = CreateNotification();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notification);

            var result = await _handler.HandleAsync(
                new GetNotificationByIdQuery(notification.Id), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(notification.Id);
            result.Recipient.Should().Be("user@example.com");
            result.Subject.Should().Be("Hello");
            result.Body.Should().Be("Body text");
            result.Status.Should().Be(NotificationStatus.Pending);
            result.Attempts.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_WhenNotificationDoesNotExist_ShouldReturnNull()
        {
            var id = Guid.NewGuid();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Notification?)null);

            var result = await _handler.HandleAsync(
                new GetNotificationByIdQuery(id), CancellationToken.None);

            result.Should().BeNull();
        }
    }
}

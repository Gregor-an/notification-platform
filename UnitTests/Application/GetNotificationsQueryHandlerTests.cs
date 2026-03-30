using Application.Interfaces.Repositories;
using Application.Notifications.Queries.GetNotifications;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace UnitTests.Application
{
    public sealed class GetNotificationsQueryHandlerTests
    {
        private readonly Mock<INotificationRepository> _repositoryMock = new();
        private readonly GetNotificationsQueryHandler _handler;

        public GetNotificationsQueryHandlerTests()
        {
            _handler = new GetNotificationsQueryHandler(_repositoryMock.Object);
        }

        private static Notification CreateNotification() => new(
            new Recipient("user@example.com"),
            new MessageContent("Subject", "Body"),
            ChannelType.Email,
            NotificationPriority.Normal);

        [Fact]
        public async Task HandleAsync_WhenNotificationsExist_ShouldReturnMappedSummaries()
        {
            var notification = CreateNotification();

            _repositoryMock
                .Setup(x => x.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<Notification> { notification }, 1));

            var (items, totalCount) = await _handler.HandleAsync(
                new GetNotificationsQuery(1, 20), CancellationToken.None);

            totalCount.Should().Be(1);
            items.Should().HaveCount(1);
            items[0].Id.Should().Be(notification.Id);
            items[0].Recipient.Should().Be("user@example.com");
            items[0].Status.Should().Be(NotificationStatus.Pending);
            items[0].AttemptCount.Should().Be(0);
        }

        [Fact]
        public async Task HandleAsync_WhenNoNotifications_ShouldReturnEmptyList()
        {
            _repositoryMock
                .Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<Notification>(), 0));

            var (items, totalCount) = await _handler.HandleAsync(
                new GetNotificationsQuery(1, 20), CancellationToken.None);

            totalCount.Should().Be(0);
            items.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_ShouldPassPaginationParamsToRepository()
        {
            _repositoryMock
                .Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<Notification>(), 0));

            await _handler.HandleAsync(new GetNotificationsQuery(3, 10), CancellationToken.None);

            _repositoryMock.Verify(
                x => x.GetPagedAsync(3, 10, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

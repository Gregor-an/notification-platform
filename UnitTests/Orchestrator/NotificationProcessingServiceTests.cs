using Application.DTOs;
using Application.Interfaces.Providers;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orchestrator.Services;
using Orchestrator.Settings;

namespace UnitTests.Orchestrator
{
    public sealed class NotificationProcessingServiceTests
    {
        private readonly Mock<INotificationRepository> _repositoryMock;
        private readonly Mock<INotificationSender> _senderMock;
        private readonly NotificationProcessingService _service;

        private static readonly NotificationProcessingOptions DefaultOptions = new()
        {
            BatchSize = 10,
            IntervalSeconds = 5
        };

        public NotificationProcessingServiceTests()
        {
            _repositoryMock = new Mock<INotificationRepository>();
            _senderMock = new Mock<INotificationSender>();
            _senderMock.Setup(x => x.ChannelType).Returns(ChannelType.Email);

            _service = new NotificationProcessingService(
                _repositoryMock.Object,
                new[] { _senderMock.Object },
                NullLogger<NotificationProcessingService>.Instance,
                Options.Create(DefaultOptions));
        }

        private static Notification CreatePendingNotification() => new(
            new Recipient("test@example.com"),
            new MessageContent("Subject", "Body"),
            ChannelType.Email,
            NotificationPriority.Normal);

        [Fact]
        public async Task ProcessPendingAsync_WhenNoPendingNotifications_ShouldNotCallSender()
        {
            _repositoryMock
                .Setup(x => x.GetPendingBatchAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Notification>());

            await _service.ProcessPendingAsync(CancellationToken.None);

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessPendingAsync_WhenSendSucceeds_ShouldMarkDelivered()
        {
            var notification = CreatePendingNotification();

            _repositoryMock
                .Setup(x => x.GetPendingBatchAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Notification> { notification });

            _senderMock
                .Setup(x => x.SendAsync(notification, It.IsAny<CancellationToken>()))
                .ReturnsAsync(SendResult.Success());

            await _service.ProcessPendingAsync(CancellationToken.None);

            notification.Status.Should().Be(NotificationStatus.Delivered);
            notification.Attempts.Should().HaveCount(1);
            notification.Attempts.First().Status.Should().Be(AttemptStatus.Succeeded);
        }

        [Fact]
        public async Task ProcessPendingAsync_WhenSendFails_ShouldMarkFailed()
        {
            var notification = CreatePendingNotification();

            _repositoryMock
                .Setup(x => x.GetPendingBatchAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Notification> { notification });

            _senderMock
                .Setup(x => x.SendAsync(notification, It.IsAny<CancellationToken>()))
                .ReturnsAsync(SendResult.Failure("Provider error."));

            await _service.ProcessPendingAsync(CancellationToken.None);

            notification.Status.Should().Be(NotificationStatus.Failed);
            notification.Attempts.Should().HaveCount(1);
            notification.Attempts.First().Status.Should().Be(AttemptStatus.Failed);
            notification.Attempts.First().FailureReason.Should().Be("Provider error.");
        }

        [Fact]
        public async Task ProcessPendingAsync_WhenNoSenderForChannel_ShouldMarkFailed()
        {
            var notification = new Notification(
                new Recipient("test@example.com"),
                new MessageContent("Subject", "Body"),
                ChannelType.Sms,
                NotificationPriority.Normal);

            _repositoryMock
                .Setup(x => x.GetPendingBatchAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Notification> { notification });

            await _service.ProcessPendingAsync(CancellationToken.None);

            notification.Status.Should().Be(NotificationStatus.Failed);
            notification.Attempts.First().FailureReason.Should()
                .Contain("No sender configured for channel");
        }

        [Fact]
        public async Task ProcessPendingAsync_WhenSendSucceeds_ShouldCallSaveChanges()
        {
            var notification = CreatePendingNotification();

            _repositoryMock
                .Setup(x => x.GetPendingBatchAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Notification> { notification });

            _senderMock
                .Setup(x => x.SendAsync(notification, It.IsAny<CancellationToken>()))
                .ReturnsAsync(SendResult.Success());

            await _service.ProcessPendingAsync(CancellationToken.None);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessPendingAsync_WhenBatchHasMultiple_ShouldProcessAll()
        {
            var notifications = new List<Notification>
            {
                CreatePendingNotification(),
                CreatePendingNotification(),
                CreatePendingNotification()
            };

            _repositoryMock
                .Setup(x => x.GetPendingBatchAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(notifications);

            _senderMock
                .Setup(x => x.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SendResult.Success());

            await _service.ProcessPendingAsync(CancellationToken.None);

            notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Delivered);
        }

        [Fact]
        public async Task ProcessPendingAsync_UsesBatchSizeFromOptions_ShouldPassCorrectBatchSize()
        {
            _repositoryMock
                .Setup(x => x.GetPendingBatchAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Notification>());

            await _service.ProcessPendingAsync(CancellationToken.None);

            _repositoryMock.Verify(
                x => x.GetPendingBatchAsync(DefaultOptions.BatchSize, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace UnitTests.Domain
{
    public sealed class NotificationTests
    {
        private static Notification CreateNotification() => new(
            new Recipient("test@example.com"),
            new MessageContent("Subject", "Body"),
            ChannelType.Email,
            NotificationPriority.Normal);

        [Fact]
        public void MarkProcessing_WhenPending_ShouldSetStatusToProcessing()
        {
            var notification = CreateNotification();

            notification.MarkProcessing();

            notification.Status.Should().Be(NotificationStatus.Processing);
        }

        [Fact]
        public void MarkProcessing_WhenFailed_ShouldSetStatusToProcessing()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt().MarkFailed("error");
            notification.MarkFailed();

            notification.MarkProcessing();

            notification.Status.Should().Be(NotificationStatus.Processing);
        }

        [Fact]
        public void MarkProcessing_WhenDelivered_ShouldThrow()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt().MarkSucceeded();
            notification.MarkDelivered();

            var act = () => notification.MarkProcessing();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void MarkProcessing_WhenAlreadyProcessing_ShouldThrow()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();

            var act = () => notification.MarkProcessing();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void StartAttempt_WhenProcessing_ShouldAddAttempt()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();

            var attempt = notification.StartAttempt();

            notification.Attempts.Should().HaveCount(1);
            attempt.AttemptNumber.Should().Be(1);
            attempt.NotificationId.Should().Be(notification.Id);
        }

        [Fact]
        public void StartAttempt_WhenNotProcessing_ShouldThrow()
        {
            var notification = CreateNotification();

            var act = () => notification.StartAttempt();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void StartAttempt_CalledMultipleTimes_ShouldIncrementAttemptNumber()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt().MarkFailed("error");
            notification.MarkFailed();
            notification.MarkProcessing();

            var secondAttempt = notification.StartAttempt();

            secondAttempt.AttemptNumber.Should().Be(2);
            notification.Attempts.Should().HaveCount(2);
        }

        [Fact]
        public void MarkDelivered_ShouldSetStatusAndProcessedUtc()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt();

            notification.MarkDelivered();

            notification.Status.Should().Be(NotificationStatus.Delivered);
            notification.ProcessedUtc.Should().NotBeNull();
        }

        [Fact]
        public void MarkFailed_ShouldSetStatusAndProcessedUtc()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt();

            notification.MarkFailed();

            notification.Status.Should().Be(NotificationStatus.Failed);
            notification.ProcessedUtc.Should().NotBeNull();
        }

        [Fact]
        public void Cancel_WhenPending_ShouldSetStatusToCancelled()
        {
            var notification = CreateNotification();

            notification.Cancel();

            notification.Status.Should().Be(NotificationStatus.Cancelled);
        }

        [Fact]
        public void Cancel_WhenDelivered_ShouldThrow()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt();
            notification.MarkDelivered();

            var act = () => notification.Cancel();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CanRetry_WhenFailedAndBelowMaxAttempts_ShouldReturnTrue()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt().MarkFailed("error");
            notification.MarkFailed();

            notification.CanRetry(maxAttempts: 3).Should().BeTrue();
        }

        [Fact]
        public void CanRetry_WhenFailedAndAtMaxAttempts_ShouldReturnFalse()
        {
            var notification = CreateNotification();

            for (int i = 0; i < 3; i++)
            {
                notification.MarkProcessing();
                notification.StartAttempt().MarkFailed("error");
                notification.MarkFailed();
            }

            notification.CanRetry(maxAttempts: 3).Should().BeFalse();
        }

        [Fact]
        public void CanRetry_WhenDelivered_ShouldReturnFalse()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt().MarkSucceeded();
            notification.MarkDelivered();

            notification.CanRetry(maxAttempts: 3).Should().BeFalse();
        }

        [Fact]
        public void CanRetry_WhenPending_ShouldReturnFalse()
        {
            var notification = CreateNotification();

            notification.CanRetry(maxAttempts: 3).Should().BeFalse();
        }

        [Fact]
        public void CanRetry_WhenProcessing_ShouldReturnFalse()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();

            notification.CanRetry(maxAttempts: 3).Should().BeFalse();
        }

        [Fact]
        public void CanRetry_WhenCancelled_ShouldReturnFalse()
        {
            var notification = CreateNotification();
            notification.Cancel();

            notification.CanRetry(maxAttempts: 3).Should().BeFalse();
        }

        [Fact]
        public void MarkDelivered_ShouldPreserveAttempts()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt().MarkSucceeded();

            notification.MarkDelivered();

            notification.Attempts.Should().HaveCount(1);
            notification.Attempts.Single().AttemptNumber.Should().Be(1);
        }

        [Fact]
        public void MarkFailed_ShouldPreserveAttempts()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();
            notification.StartAttempt().MarkFailed("error");

            notification.MarkFailed();

            notification.Attempts.Should().HaveCount(1);
            notification.Attempts.Single().AttemptNumber.Should().Be(1);
        }
    }
}
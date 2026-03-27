using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Infrastructure
{
    public sealed class NotificationRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _dbContext;
        private readonly NotificationRepository _repository;

        public NotificationRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _dbContext = CreateDbContext();
            _dbContext.Database.EnsureCreated();

            _repository = new NotificationRepository(_dbContext);
        }

        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new AppDbContext(options);
        }

        private static Notification CreateNotification(string recipient = "test@example.com") => new(
            new Recipient(recipient),
            new MessageContent("Subject", "Body"),
            ChannelType.Email,
            NotificationPriority.Normal);

        [Fact]
        public async Task AddAsync_ShouldPersistNotification()
        {
            var notification = CreateNotification();

            await _repository.AddAsync(notification, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            await using var verificationContext = CreateDbContext();
            var saved = await verificationContext.Notifications.FindAsync(notification.Id);

            saved.Should().NotBeNull();
            saved!.Status.Should().Be(NotificationStatus.Pending);
            saved.Recipient.Value.Should().Be("test@example.com");
            saved.Content.Subject.Should().Be("Subject");
            saved.Content.Body.Should().Be("Body");
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ShouldReturnNotification()
        {
            var notification = CreateNotification();

            await _repository.AddAsync(notification, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            await using var readContext = CreateDbContext();
            var repository = new NotificationRepository(readContext);

            var result = await repository.GetByIdAsync(notification.Id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(notification.Id);
            result.Recipient.Value.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
        {
            await using var readContext = CreateDbContext();
            var repository = new NotificationRepository(readContext);

            var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPendingBatchAsync_ShouldReturnOnlyPending()
        {
            var pending = CreateNotification("pending@example.com");
            var processing = CreateNotification("processing@example.com");
            processing.MarkProcessing();

            await _repository.AddAsync(pending, CancellationToken.None);
            await _repository.AddAsync(processing, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            await using var readContext = CreateDbContext();
            var repository = new NotificationRepository(readContext);

            var result = await repository.GetPendingBatchAsync(10, CancellationToken.None);

            result.Should().HaveCount(1);
            result.Single().Id.Should().Be(pending.Id);
            result.Single().Status.Should().Be(NotificationStatus.Pending);
        }

        [Fact]
        public async Task GetPendingBatchAsync_ShouldRespectBatchSize()
        {
            for (var i = 0; i < 5; i++)
            {
                await _repository.AddAsync(CreateNotification($"user{i}@example.com"), CancellationToken.None);
            }

            await _repository.SaveChangesAsync(CancellationToken.None);

            await using var readContext = CreateDbContext();
            var repository = new NotificationRepository(readContext);

            var result = await repository.GetPendingBatchAsync(3, CancellationToken.None);

            result.Should().HaveCount(3);
            result.Should().OnlyContain(x => x.Status == NotificationStatus.Pending);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldLoadAttempts()
        {
            var notification = CreateNotification();
            notification.MarkProcessing();

            var attempt = notification.StartAttempt();
            attempt.MarkSucceeded();

            await _repository.AddAsync(notification, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            await using var readContext = CreateDbContext();
            var repository = new NotificationRepository(readContext);

            var result = await repository.GetByIdAsync(notification.Id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Attempts.Should().HaveCount(1);
        }

        [Fact]
        public async Task SaveChangesAsync_AfterStatusChange_ShouldPersistNewStatus()
        {
            var notification = CreateNotification();

            await _repository.AddAsync(notification, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            notification.MarkProcessing();
            notification.StartAttempt().MarkSucceeded();
            notification.MarkDelivered();

            await _repository.SaveChangesAsync(CancellationToken.None);

            await using var verificationContext = CreateDbContext();
            var updated = await verificationContext.Notifications.FindAsync(notification.Id);

            updated.Should().NotBeNull();
            updated!.Status.Should().Be(NotificationStatus.Delivered);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Dispose();
        }
    }
}
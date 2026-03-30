using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _dbContext;

        public NotificationRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
        {
            await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        }

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return _dbContext.Notifications
                .Include(x => x.Attempts)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _dbContext.Notifications.Include(x => x.Attempts);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreatedUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public Task<List<Notification>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken)
        {
            return _dbContext.Notifications
                .Include(x => x.Attempts)
                .Where(x => x.Status == NotificationStatus.Pending)
                .OrderBy(x => x.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Notification>> GetRetryBatchAsync(
            int batchSize, int maxAttempts, CancellationToken cancellationToken)
        {
            return _dbContext.Notifications
                .Include(x => x.Attempts)
                .Where(x => x.Status == NotificationStatus.Failed && x.Attempts.Count < maxAttempts)
                .OrderBy(x => x.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

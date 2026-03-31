using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken cancellationToken);
        Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<(List<Notification> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
        Task<List<Notification>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken);
        Task<List<Notification>> GetRetryBatchAsync(int batchSize, int maxAttempts, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
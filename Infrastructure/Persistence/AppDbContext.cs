using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public sealed class AppDbContext : DbContext
    {
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}

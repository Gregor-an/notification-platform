using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
    {
        public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AttemptNumber).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.CreatedUtc).IsRequired();

            builder.HasOne<Notification>()
                .WithMany(nameof(Notification.Attempts))
                .HasForeignKey(x => x.NotificationId);
        }
    }
}

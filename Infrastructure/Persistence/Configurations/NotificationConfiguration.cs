using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Enums;

namespace Infrastructure.Persistence.Configurations
{
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ChannelType).IsRequired();
            builder.Property(x => x.Priority).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.CreatedUtc).IsRequired();

            builder.OwnsOne(x => x.Recipient, recipient =>
            {
                recipient.Property(r => r.Value)
                    .HasColumnName("Recipient")
                    .HasMaxLength(256)
                    .IsRequired();
            });

            builder.OwnsOne(x => x.Content, content =>
            {
                content.Property(c => c.Subject)
                    .HasColumnName("Subject")
                    .HasMaxLength(200);

                content.Property(c => c.Body)
                    .HasColumnName("Body")
                    .IsRequired();
            });

            builder.HasMany(x => x.Attempts)
                .WithOne()
                .HasForeignKey(x => x.NotificationId);

            builder.Navigation(x => x.Attempts)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

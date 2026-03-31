using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Contracts.Requests
{
    public sealed class CreateNotificationRequest
    {
        [Required]
        [MaxLength(256)]
        public string Recipient { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public ChannelType ChannelType { get; set; }

        public NotificationPriority Priority { get; set; }
    }
}
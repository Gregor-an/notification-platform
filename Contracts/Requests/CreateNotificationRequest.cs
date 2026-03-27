using System.ComponentModel.DataAnnotations;

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

        [Range(1, 3)]
        public int ChannelType { get; set; }

        [Range(0, 2)]
        public int Priority { get; set; }
    }
}
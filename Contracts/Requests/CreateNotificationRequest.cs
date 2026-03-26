
namespace Contracts.Requests
{
    public sealed class CreateNotificationRequest
    {
        public string Recipient { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int ChannelType { get; set; }
        public int Priority { get; set; }
    }
}
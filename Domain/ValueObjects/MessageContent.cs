
namespace Domain.ValueObjects
{
    public sealed class MessageContent
    {
        public string Subject { get; private set; } = null!;
        public string Body { get; private set; } = null!;

        private MessageContent() { }

        public MessageContent(string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Message body cannot be empty.", nameof(body));

            Subject = subject?.Trim() ?? string.Empty;
            Body = body.Trim();
        }
    }
}

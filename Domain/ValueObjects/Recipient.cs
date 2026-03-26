
namespace Domain.ValueObjects
{
    public sealed class Recipient
    {
        public string Value { get; private set; }

        private Recipient() { }

        public Recipient(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Recipient cannot be empty.", nameof(value));

            Value = value.Trim();
        }
    }
}

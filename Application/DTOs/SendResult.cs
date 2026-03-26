
namespace Application.DTOs
{
    public sealed class SendResult
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }

        public static SendResult Success() => new() { IsSuccess = true };
        public static SendResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
    }
}
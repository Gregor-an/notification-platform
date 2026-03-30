using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.Requests;
using Contracts.Responses;

namespace Web.Services;

public class NotificationApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<GetNotificationsResponse?> GetNotificationsAsync(int page, int pageSize, CancellationToken ct = default)
        => http.GetFromJsonAsync<GetNotificationsResponse>(
            $"api/notifications?page={page}&pageSize={pageSize}", JsonOptions, ct);

    public Task<GetNotificationByIdResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => http.GetFromJsonAsync<GetNotificationByIdResponse>(
            $"api/notifications/{id}", JsonOptions, ct);

    public Task<HttpResponseMessage> CreateAsync(CreateNotificationRequest request, CancellationToken ct = default)
        => http.PostAsJsonAsync("api/notifications", request, ct);
}

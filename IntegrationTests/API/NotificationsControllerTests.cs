using System.Net;
using System.Net.Http.Json;
using Contracts.Requests;
using Contracts.Responses;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.API
{
    public sealed class NotificationsControllerTests
        : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public NotificationsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private static CreateNotificationRequest BuildRequest(
            string recipient = "test@example.com") => new()
            {
                Recipient = recipient,
                Subject = "Test Subject",
                Body = "Test Body",
                ChannelType = (int)ChannelType.Email,
                Priority = (int)NotificationPriority.Normal
            };

        [Fact]
        public async Task POST_Notifications_ShouldReturn200_AndReturnId()
        {
            var response = await _client.PostAsJsonAsync("/api/notifications", BuildRequest());

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CreateNotificationResponse>();
            result.Should().NotBeNull();
            result!.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task POST_Notifications_ShouldPersistToDatabase()
        {
            var response = await _client.PostAsJsonAsync("/api/notifications", BuildRequest());
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CreateNotificationResponse>();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var saved = await dbContext.Notifications.FindAsync(result!.Id);

            saved.Should().NotBeNull();
            saved!.Status.Should().Be(NotificationStatus.Pending);
        }

        [Fact]
        public async Task POST_Notifications_WithEmptyBody_ShouldReturn400()
        {
            var response = await _client.PostAsJsonAsync("/api/notifications", new { });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task POST_Notifications_ShouldSetCorrectRecipient()
        {
            var response = await _client
                .PostAsJsonAsync("/api/notifications", BuildRequest("recipient@example.com"));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CreateNotificationResponse>();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await dbContext.Notifications.FindAsync(result!.Id);

            saved.Should().NotBeNull();
            saved!.Recipient.Value.Should().Be("recipient@example.com");
        }

        public void Dispose() => _client.Dispose();
    }
}
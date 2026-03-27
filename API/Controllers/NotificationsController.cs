using Application.Notifications.Commands.CreateNotification;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Contracts.Requests;
using Contracts.Responses;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class NotificationsController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<CreateNotificationResponse>> Create(
            [FromBody] CreateNotificationRequest request,
            [FromServices] CreateNotificationCommandHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new CreateNotificationCommand(
                request.Recipient,
                request.Subject,
                request.Body,
                request.ChannelType,
                request.Priority);

            var id = await handler.HandleAsync(command, cancellationToken);

            return Ok(new CreateNotificationResponse { Id = id });
        }
    }
}

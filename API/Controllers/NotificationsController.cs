using Application.Notifications.Commands.CreateNotification;
using Application.Notifications.Queries.GetNotificationById;
using Application.Notifications.Queries.GetNotifications;
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

        [HttpGet]
        public async Task<ActionResult<GetNotificationsResponse>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromServices] GetNotificationsQueryHandler handler = null!,
            CancellationToken cancellationToken = default)
        {
            var query = new GetNotificationsQuery(page, pageSize);
            var (items, totalCount) = await handler.HandleAsync(query, cancellationToken);

            return Ok(new GetNotificationsResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GetNotificationByIdResponse>> GetById(
            Guid id,
            [FromServices] GetNotificationByIdQueryHandler handler,
            CancellationToken cancellationToken)
        {
            var query = new GetNotificationByIdQuery(id);
            var notification = await handler.HandleAsync(query, cancellationToken);

            if (notification is null)
                return NotFound();

            return Ok(new GetNotificationByIdResponse { Notification = notification });
        }
    }
}

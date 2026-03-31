using Contracts.Requests;
using FluentValidation;

namespace API.Validators
{
    public sealed class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
    {
        public CreateNotificationRequestValidator()
        {
            RuleFor(x => x.Recipient)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(x => x.Subject)
                .MaximumLength(200);

            RuleFor(x => x.Body)
                .NotEmpty();

            RuleFor(x => x.ChannelType)
                .IsInEnum();

            RuleFor(x => x.Priority)
                .IsInEnum();
        }
    }
}
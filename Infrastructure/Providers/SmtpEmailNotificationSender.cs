using Application.DTOs;
using Application.Interfaces.Providers;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Providers
{
    public sealed class SmtpEmailNotificationSender : INotificationSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailNotificationSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public ChannelType ChannelType => ChannelType.Email;

        public async Task<SendResult> SendAsync(Notification notification, CancellationToken cancellationToken)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
                message.To.Add(MailboxAddress.Parse(notification.Recipient.Value));
                message.Subject = notification.Content.Subject;
                message.Body = new TextPart("plain") { Text = notification.Content.Body };

                using var client = new SmtpClient();

                var socketOptions = _options.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTlsWhenAvailable;

                await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(quit: true, cancellationToken);

                return SendResult.Success();
            }
            catch (Exception ex)
            {
                return SendResult.Failure(ex.Message);
            }
        }
    }
}

using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using SharedIdentity.Abstractions;

namespace SharedIdentity.Services
{
    public class SendGridEmailService : IEmailServiceProvider

    {
        private readonly IConfiguration _config;
        public SendGridEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            SendGridClient client = GetSendGridClient();
            var from = new EmailAddress("rafikiass@hotmail.com", "dotnet identity");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlMessage, htmlMessage);

            await client.SendEmailAsync(msg);
        }

        public async Task Send2faCodeEmailAsync(string email, string code)
        {
            SendGridClient client = GetSendGridClient();
            var from = new EmailAddress("rafikiass@hotmail.com", "dotnet identity");
            var to = new EmailAddress(email);
            var subject = "Your two factor auth code";
            var message = "Your verification code is :" + code;

            var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, message, message);

            await client.SendEmailAsync(sendGridMessage);
        }

        private SendGridClient GetSendGridClient()
        {
            var apiKey = _config["SendGridEmail:Apikey"];
            return new SendGridClient(apiKey);
        }


    }
}

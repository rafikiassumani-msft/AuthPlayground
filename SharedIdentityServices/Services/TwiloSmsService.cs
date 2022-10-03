using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedIdentity.Abstractions;
using SharedIdentity.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;

namespace SharedIdentity.Services
{
    public class TwiloSmsService : ISmsServiceProvider
    {

        private readonly TwilioSettings _twilioSettings;
        private readonly ILogger<TwiloSmsService> _logger;  

        public TwiloSmsService(IOptions<TwilioSettings> twilioSettings, ILogger<TwiloSmsService> logger )
        {
            _twilioSettings = twilioSettings.Value;
            _logger = logger;
            InitTwilioClient();
        }
        public async Task<MessageResult> SendSmsAnsync(string phoneNumber, string code)
        {
            var smsResult = await MessageResource.CreateAsync(
                body: "Your authentication code is: " + code,
                from: new Twilio.Types.PhoneNumber(_twilioSettings.TwilioPhoneNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            if (smsResult == null)
            {
                return new MessageResult(Status: "Failed");
            }

            return new MessageResult(Status: smsResult.Status.ToString());
        }

        public async Task<MessageResult> SendVerificationCodeAsync(string phoneNumber)
        {
            try
            {
                var smsResult = await VerificationResource.CreateAsync(
                    pathServiceSid: _twilioSettings.VerificationServiceSID, 
                    to: phoneNumber, 
                    channel: "sms"
                );

                if (smsResult == null)
                {
                    return new MessageResult(Status: "Failed");
                }

                return new MessageResult(Status: smsResult.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError("failed to validate phone number", ex);
                return new MessageResult(Status: "Failed");
            }

        }

        public async Task<MessageResult> VerifyPhoneNumberAsync(string phoneNumber, string verificationCode)
        {
            var verificationResult = await VerificationCheckResource.CreateAsync(
                to: phoneNumber,
                code: verificationCode,
                pathServiceSid: _twilioSettings.VerificationServiceSID
            );

            if (verificationResult == null)
            {
                return new MessageResult(Status: "Failed");
            }

            return new MessageResult(Status: verificationResult.Status);
        }

        private void InitTwilioClient()
        {
            TwilioClient.Init(_twilioSettings.AccountSID, _twilioSettings.AuthToken);
        }
    }
}
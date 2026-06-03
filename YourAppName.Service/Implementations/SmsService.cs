using Microsoft.Extensions.Configuration;
using YourAppName.Service.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace YourAppName.Service.Implementations
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _config;

        public SmsService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendSmsAsync(string phoneNumber, string messageBody, CancellationToken cancellationToken)
        {
            // Check for cancellation at the very beginning: 
            // If the request is already cancelled, stop immediately.
            cancellationToken.ThrowIfCancellationRequested();


            var twilioSettings = _config.GetSection("TwilioSettings");
            var accountSid = twilioSettings["AccountSID"];
            var authToken = twilioSettings["AuthToken"];
            var fromPhoneNumber = twilioSettings["FromPhoneNumber"];

            TwilioClient.Init(accountSid, authToken);

            // Check for cancellation one more time right before the actual API call to Twilio
            cancellationToken.ThrowIfCancellationRequested();

            await MessageResource.CreateAsync(
                body: messageBody,
                from: new PhoneNumber(fromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );
        }
    }
}

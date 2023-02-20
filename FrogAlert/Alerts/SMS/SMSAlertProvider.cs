using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace FrogAlert.Alerts.SMS
{
    public class SMSAlertProvider : IAlertProvider
    {
        private readonly ILogger<SMSAlertProvider> _logger;

        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _messagingServiceSID;
        private readonly PhoneNumber[] _alertPhoneNumbers;

        public SMSAlertProvider(ILogger<SMSAlertProvider> logger, IConfiguration configuration)
        {
            _logger = logger;

            _accountSid = configuration.GetSection("Twilio").GetValue<string>("AccountSID");
            _authToken = configuration.GetSection("Twilio").GetValue<string>("AuthToken");

            TwilioClient.Init(_accountSid, _authToken);

            _messagingServiceSID = configuration.GetSection("Twilio").GetValue<string>("MessagingServiceSID");

            _alertPhoneNumbers = configuration.GetSection("Alerts").GetSection("PhoneNumbers").Get<string[]>()
                .Select(x => new PhoneNumber(x)).ToArray();
        }

        public async Task SendAlert(string message)
        {
            _logger.LogInformation("Sending SMS alert. Message length: {MessageLength}", message.Length);

            foreach (var phoneNumber in _alertPhoneNumbers)
            {
                try
                {
                    var messageOptions = new CreateMessageOptions(phoneNumber)
                    {
                        MessagingServiceSid = _messagingServiceSID,
                        Body = message
                    };

                    var messageResource = await MessageResource.CreateAsync(messageOptions);

                    if (messageResource.ErrorCode.HasValue)
                    {
                        _logger.LogError("API error occurred during sending SMS alert: {ErrorCode} - {ErrorMessage}",
                            messageResource.ErrorCode.Value, messageResource.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error occurred during sending SMS alert.");
                }
            }
        }
    }
}

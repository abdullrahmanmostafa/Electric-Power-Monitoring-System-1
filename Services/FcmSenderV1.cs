using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Electric_Power_Monitoring_System.Services
{
    public class FcmSenderV1 : IFcmSender
    {
        private readonly ILogger<FcmSenderV1> _logger;

        public FcmSenderV1(ILogger<FcmSenderV1> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message()
                {
                    Token = fcmToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>()
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Successfully sent FCM message: {Response}", response);
                return true;
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, "Error sending FCM notification");
                return false;
            }
        }
    }
}
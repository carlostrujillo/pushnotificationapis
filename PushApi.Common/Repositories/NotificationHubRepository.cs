using Microsoft.Azure.NotificationHubs;
using PushApi.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PushApi.Common.Repositories
{
    public class NotificationHubRepository
    {
        NotificationHubClient _hub;

        public NotificationHubRepository(string listenConnString, string hubName)
        {
            var notificationHub =  new NotificationHub(listenConnString, hubName);
            _hub = notificationHub.Hub;
        }

        public async Task<IList<RegistrationDescription>> Get()
        {
            var registrationDescriptions = await _hub.GetAllRegistrationsAsync(10);
            IList<RegistrationDescription> retval = new List<RegistrationDescription>();

            foreach (var regdesc in registrationDescriptions)
            {
                retval.Add(regdesc);
            }
            return retval;
        }

        public async Task<RegistrationDescription> Get(string id)
        {
            var registrationDescription = await _hub.GetRegistrationAsync<RegistrationDescription>(id);

            return registrationDescription;
        }

        public async Task Delete(string registrationId)
        {
            await _hub.DeleteRegistrationAsync(registrationId);
        }

        public async Task<string> Upsert(PushRegistration deviceUpdate)
        {
            RegistrationDescription desc = null;

            switch (deviceUpdate.Platform)
            {
                case Platform.Windows:
                    desc = new WindowsRegistrationDescription(deviceUpdate.DeviceToken);
                    break;
                case Platform.iOS:
                    desc = new AppleRegistrationDescription(deviceUpdate.DeviceToken);
                    break;
                case Platform.Android:
                    desc = new GcmRegistrationDescription(deviceUpdate.DeviceToken);
                    break;
                default:
                    throw new ArgumentException("Wrong PushChannel");
            }

            string registrationId = deviceUpdate.RegistrationId;
            if (string.IsNullOrEmpty(registrationId))
            {
                registrationId = await _hub.CreateRegistrationIdAsync();
            }
            desc.RegistrationId = registrationId;

            desc.Tags = new HashSet<string>(deviceUpdate.Tags);

            var registration = await _hub.CreateOrUpdateRegistrationAsync(desc);
            
            return registrationId;
        }

        public async Task<List<NotificationOutcome>> Send(SendPayload payload)
        {
            var pns = new List<Platform>
            {
                Platform.iOS,
                Platform.Android
            };

            return await Send(payload, pns);
        }

        public async Task<List<NotificationOutcome>> Send(SendPayload payload, List<Platform> pns)
        {
            string tagExpression = "";

            if (payload.Tags != null && payload.Tags.Count > 0)
            {
                foreach (var tag in payload.Tags)
                {
                    tagExpression += tag;
                    tagExpression += "||";
                }

                tagExpression = tagExpression.Substring(0, tagExpression.Length - 2);
            }
            else if(!string.IsNullOrEmpty(payload.TagExpression))
            {
                tagExpression = payload.TagExpression;
            }

            if (!tagExpression.Contains("UserId") && !string.IsNullOrEmpty(payload.UserId))
            {
                if (!string.IsNullOrEmpty(tagExpression)) tagExpression += "||";
                tagExpression += $"UserId:{payload.UserId}";
            }

            var outcomes = new List<NotificationOutcome>();

            if (pns.Contains(Platform.iOS))
            {
                var pushMsg = "{\"aps\":{\"alert\":\"" + payload.Message + "\", \"sound\":\"default\"}}";
                var outcome = await _hub.SendAppleNativeNotificationAsync(pushMsg, tagExpression);

                outcomes.Add(outcome);
            }
            if (pns.Contains(Platform.Android))
            {
                var pushMsg = "{\"notification\": {\"body\": \"" + payload.Message + "\", \"sound\" :\"default\" }}";
                var outcome = await _hub.SendGcmNativeNotificationAsync(pushMsg, tagExpression);

                outcomes.Add(outcome);
            }

            return outcomes;
        }
    }
}

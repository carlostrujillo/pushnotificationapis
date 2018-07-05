using Microsoft.Azure.NotificationHubs;
using System.Configuration;

namespace PushApi.Common.Repositories
{
    public class NotificationHub
    {
        public NotificationHubClient Hub { get; set; }

        public NotificationHub(string listenConnString,string hubName)
        {

            Hub = NotificationHubClient.CreateClientFromConnectionString(listenConnString, hubName);
        }
    }
}

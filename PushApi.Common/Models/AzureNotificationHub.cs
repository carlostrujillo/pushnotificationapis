using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushApi.Common.Models
{
    public class AzureNotificationHub
    {
        public string AppId { get; set; }
        public string Endpoint { get; set; }

        public string HubName { get; set; }

    }
}

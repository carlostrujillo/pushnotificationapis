using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace PushApi.Common.Models
{
    public class PushRegistration
    {
        public string RegistrationId
        { get; set; }

        public string AppId
        { get; set; }

        public string UserId
        { get; set; }

        public string DeviceToken
        { get; set; }

        public Platform Platform
        { get; set; }

        public List<string> Tags
        { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Platform
    {
        none,
        iOS,
        Android,
        Windows
    }
}
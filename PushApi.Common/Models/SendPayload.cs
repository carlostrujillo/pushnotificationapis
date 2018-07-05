using System.Collections.Generic;

namespace PushApi.Common.Models
{
    public class SendPayload
    {
        public string Message
        { get; set; }

        public string AppId
        { get; set; }

        public string UserId
        { get; set; }

        public string TagExpression
        { get; set; }

        public List<string> Tags
        { get; set; }
    }
}
using System.Runtime.Serialization;
using Umbraco.Web.UI;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "notification", Namespace = "")]
    public class Notification
    {
        [DataMember(Name = "header")]
        public string Header { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
        [DataMember(Name = "type")]
        public SpeechBubbleIcon NotificationType { get; set; }
    }
}
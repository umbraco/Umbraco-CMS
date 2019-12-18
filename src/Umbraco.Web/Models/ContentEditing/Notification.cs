using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "notification", Namespace = "")]
    public class Notification
    {
        public Notification()
        {

        }

        public Notification(string header, string message, NotificationStyle notificationType)
        {
            Header = header;
            Message = message;
            NotificationType = notificationType;
        }

        [DataMember(Name = "header")]
        public string Header { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "type")]
        public NotificationStyle NotificationType { get; set; }
        
    }
}

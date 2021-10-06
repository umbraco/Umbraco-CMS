using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.UI
{
    public class ImageComponent : INotificationHandler<MediaSavingNotification>
    {
        public void Handle(MediaSavingNotification notification)
        {

            foreach (var entity in notification.SavedEntities)
            {
                if (entity.Name == "Bible")
                {
                    notification.Messages.Add(new EventMessage("Error", "Error Message", EventMessageType.Error));
                }

                if (entity.Name == "Countryside")
                {
                    notification.Messages.Add(new EventMessage("Info", "Info Message", EventMessageType.Info));
                    notification.Messages.Add(new EventMessage("Default", "Default Message"));
                    notification.Messages.Add(new EventMessage("Success", "Success Message", EventMessageType.Success));
                    notification.Messages.Add(new EventMessage("Warning", "Warning Message", EventMessageType.Warning));
                }


            }
        }
    }
}

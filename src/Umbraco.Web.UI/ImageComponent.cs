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

                notification.Messages.Add(new EventMessage("Category", "Error Message", EventMessageType.Error));

                //notification.Messages.Add(new EventMessage("Category", "Info Message", EventMessageType.Info));
                //notification.Messages.Add(new EventMessage("Category", "Default Message"));
                //notification.Messages.Add(new EventMessage("Category", "Success Message", EventMessageType.Success));
                //notification.Messages.Add(new EventMessage("Category", "Warning Message", EventMessageType.Warning));

            }
        }
    }
}

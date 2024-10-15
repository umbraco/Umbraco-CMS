using System.Collections;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Notifications;

public class SendingChildrenNotification : INotification
{
    public SendingChildrenNotification(IList children)
    {
        Children = children;
    }

    public IList Children { get; set; }
}

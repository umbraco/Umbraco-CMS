using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DataTypeDeletingNotification : DeletingNotification<IDataType>
{
    public DataTypeDeletingNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }
}

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DataTypeDeletedNotification : DeletedNotification<IDataType>
{
    public DataTypeDeletedNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }
}

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DataTypeMovingNotification : MovingNotification<IDataType>
{
    public DataTypeMovingNotification(MoveEventInfo<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

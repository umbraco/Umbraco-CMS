using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the DataTypeService when the Move method is called in the API.
/// </summary>
public class DataTypeMovingNotification : MovingNotification<IDataType>
{
    public DataTypeMovingNotification(MoveEventInfo<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

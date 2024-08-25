using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the DataTypeService when the Move method is called in the API, after the IDataType has been moved.
/// </summary>
public class DataTypeMovedNotification : MovedNotification<IDataType>
{
    public DataTypeMovedNotification(MoveEventInfo<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

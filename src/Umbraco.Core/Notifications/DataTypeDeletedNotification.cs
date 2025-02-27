using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the DataTypeService when the Delete method is called in the API, after the IDataType objects have been deleted.
/// </summary>
public class DataTypeDeletedNotification : DeletedNotification<IDataType>
{
    public DataTypeDeletedNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }
}

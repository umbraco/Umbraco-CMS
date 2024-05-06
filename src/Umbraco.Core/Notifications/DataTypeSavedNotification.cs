using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the DataTypeService when the Save method is called in the API, and after data has been persisted.
/// </summary>
public class DataTypeSavedNotification : SavedNotification<IDataType>
{
    public DataTypeSavedNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DataTypeSavedNotification(IEnumerable<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

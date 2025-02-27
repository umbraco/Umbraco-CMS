using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the DataTypeService when the Save method is called in the API.
/// </summary>
public class DataTypeSavingNotification : SavingNotification<IDataType>
{
    public DataTypeSavingNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DataTypeSavingNotification(IEnumerable<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

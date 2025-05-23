using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the DataTypeService when the Delete method is called in the API.
/// </summary>
public class DataTypeDeletingNotification : DeletingNotification<IDataType>
{
    public DataTypeDeletingNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }
}

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

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

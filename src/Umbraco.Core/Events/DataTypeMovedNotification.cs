using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class DataTypeMovedNotification : MovedNotification<IDataType>
    {
        public DataTypeMovedNotification(MoveEventInfo<IDataType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class DataTypeMovingNotification : MovingNotification<IDataType>
    {
        public DataTypeMovingNotification(MoveEventInfo<IDataType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}

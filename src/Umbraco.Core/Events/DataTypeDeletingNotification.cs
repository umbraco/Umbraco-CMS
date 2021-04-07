using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class DataTypeDeletingNotification : DeletingNotification<IDataType>
    {
        public DataTypeDeletingNotification(IDataType target, EventMessages messages) : base(target, messages)
        {
        }
    }
}

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class DataTypeDeletedNotification : DeletedNotification<IDataType>
    {
        public DataTypeDeletedNotification(IDataType target, EventMessages messages) : base(target, messages)
        {
        }
    }
}

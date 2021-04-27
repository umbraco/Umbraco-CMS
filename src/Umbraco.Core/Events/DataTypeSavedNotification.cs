using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class DataTypeSavedNotification : SavedNotification<IDataType>
    {
        public DataTypeSavedNotification(IDataType target, EventMessages messages) : base(target, messages)
        {
        }

        public DataTypeSavedNotification(IEnumerable<IDataType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}

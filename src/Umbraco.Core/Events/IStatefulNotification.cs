using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public interface IStatefulNotification : INotification
    {
        IDictionary<string, object> State { get; set; }
    }
}

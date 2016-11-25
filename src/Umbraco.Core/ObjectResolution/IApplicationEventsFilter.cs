using System.Collections.Generic;

namespace Umbraco.Core.ObjectResolution
{
    /// <summary>
    /// This can be used to filter or re-order application events before they are executed
    /// </summary>
    public interface IApplicationEventsFilter
    {
        void Filter(List<IApplicationEventHandler> eventHandlers);
    }
}
using System;
using System.Collections.Generic;

namespace Umbraco.Core.ObjectResolution
{
    public class ApplicationEventsEventArgs : EventArgs
    {
        public List<IApplicationEventHandler> Handlers { get; private set; }

        public ApplicationEventsEventArgs(List<IApplicationEventHandler> handlers)
        {
            Handlers = handlers;
        }
    }
}
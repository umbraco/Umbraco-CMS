using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    public interface IScope : IDisposeOnRequestEnd // implies IDisposable
    {
        UmbracoDatabase Database { get; }
        IList<EventMessage> Messages { get; }

        void Complete();
    }
}

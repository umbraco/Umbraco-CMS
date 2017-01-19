using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Represents a scope.
    /// </summary>
    public interface IScope : IDisposable
    {
        /// <summary>
        /// Gets the scope database.
        /// </summary>
        UmbracoDatabase Database { get; }

        /// <summary>
        /// Gets the scope event messages.
        /// </summary>
        IList<EventMessage> Messages { get; }

        /// <summary>
        /// Gets the event manager
        /// </summary>
        IEventManager EventManager { get; }

        /// <summary>
        /// Completes the scope.
        /// </summary>
        void Complete();

#if DEBUG_SCOPES
        Guid InstanceId { get; }
#endif
    }
}

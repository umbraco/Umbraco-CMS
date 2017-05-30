using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Represents a scope.
    /// </summary>
    public interface IScope : IDisposable, IInstanceIdentifiable
    {
        /// <summary>
        /// Gets the scope database.
        /// </summary>
        UmbracoDatabase Database { get; }

        /// <summary>
        /// Gets the scope event messages.
        /// </summary>
        EventMessages Messages { get; }

        /// <summary>
        /// Gets the event manager
        /// </summary>
        IEventDispatcher Events { get; }

        /// <summary>
        /// Gets the repository cache mode.
        /// </summary>
        RepositoryCacheMode RepositoryCacheMode { get; }

        /// <summary>
        /// Gets the isolated cache.
        /// </summary>
        IsolatedRuntimeCache IsolatedRuntimeCache { get; }

        /// <summary>
        /// Completes the scope.
        /// </summary>
        /// <returns>A value indicating whether the scope has been successfully completed.</returns>
        /// <remarks>Can return false if any child scope has not completed.</remarks>
        bool Complete();
    }
}

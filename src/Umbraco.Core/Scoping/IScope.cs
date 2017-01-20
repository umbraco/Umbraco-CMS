using System;
using System.Collections.Generic;
using Umbraco.Core.Cache;
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
        IEventManager Events { get; }

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
        void Complete();

        /// <summary>
        /// Registers an action to execute on exit.
        /// </summary>
        /// <param name="key">The unique key of the action.</param>
        /// <param name="action">The action.</param>
        /// <remarks>
        /// <para>The key is unique (as in, dictionary key).</para>
        /// <para>The action will execute only if the scope completes.</para>
        /// </remarks>
        void OnExit(string key, Action action);

        /// <remarks>
        /// <para>The key is unique (as in, dictionary key).</para>
        /// <para>The action always executes, with an argument indicating whether the scope completed.</para>
        /// </remarks>
        void OnExit(string key, Action<bool> action);

#if DEBUG_SCOPES
        Guid InstanceId { get; }
#endif
    }
}

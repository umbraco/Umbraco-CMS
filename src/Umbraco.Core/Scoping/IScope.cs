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
        /// Enlists an object into the scope.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="key">The unique key.</param>
        /// <param name="creator">A method creating the object.</param>
        /// <returns>The object.</returns>
        T Enlist<T>(string key, Func<T> creator);

        /// <summary>
        /// Enlists an action into the scope.
        /// </summary>
        /// <param name="key">The unique key.</param>
        /// <param name="actionTimes">When to execute the action.</param>
        /// <param name="action">The action to execute.</param>
        void Enlist(string key, ActionTime actionTimes, Action<ActionTime, bool> action);

        /// <summary>
        /// Enlists an object and an action into the scope.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="key">The unique key.</param>
        /// <param name="creator">A method creating the object.</param>
        /// <param name="actionTimes">When to execute the action.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>The object.</returns>
        T Enlist<T>(string key, Func<T> creator, ActionTime actionTimes, Action<ActionTime, bool, T> action);

#if DEBUG_SCOPES
        Guid InstanceId { get; }
#endif
    }
}

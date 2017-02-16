using System;
using System.Data;
using Umbraco.Core.Events;
#if DEBUG_SCOPES
using System.Collections.Generic;
#endif

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Provides scopes.
    /// </summary>
    public interface IScopeProvider
    {
        /// <summary>
        /// Creates an ambient scope.
        /// </summary>
        /// <returns>The created ambient scope.</returns>
        /// <remarks>
        /// <para>The created scope becomes the ambient scope.</para>
        /// <para>If an ambient scope already exists, it becomes the parent of the created scope.</para>
        /// <para>When the created scope is disposed, the parent scope becomes the ambient scope again.</para>
        /// </remarks>
        IScope CreateScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false);

        /// <summary>
        /// Creates a detached scope.
        /// </summary>
        /// <returns>A detached scope.</returns>
        /// <remarks>
        /// <para>A detached scope is not ambient and has no parent.</para>
        /// <para>It is meant to be attached by <see cref="AttachScope"/>.</para>
        /// </remarks>
        IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null);

        /// <summary>
        /// Attaches a scope.
        /// </summary>
        /// <param name="scope">The scope to attach.</param>
        /// <param name="callContext">A value indicating whether to force usage of call context.</param>
        /// <remarks>
        /// <para>Only a scope created by <see cref="CreateDetachedScope"/> can be attached.</para>
        /// </remarks>
        void AttachScope(IScope scope, bool callContext = false);

        /// <summary>
        /// Detaches a scope.
        /// </summary>
        /// <returns>The detached scope.</returns>
        /// <remarks>
        /// <para>Only a scope previously attached by <see cref="AttachScope"/> can be detached.</para>
        /// </remarks>
        IScope DetachScope();

        /// <summary>
        /// Gets the scope context.
        /// </summary>
        ScopeContext Context { get; }

#if DEBUG_SCOPES
        Dictionary<Guid, object> CallContextObjects { get; }
        IEnumerable<ScopeInfo> ScopeInfos { get; }
        ScopeInfo GetScopeInfo(IScope scope);
#endif
    }
}

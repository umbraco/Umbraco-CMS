using System.Data;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Scoping;

/// <summary>
///     Provides scopes.
/// </summary>
public interface IScopeProvider : ICoreScopeProvider
{
    /// <inheritdoc />
    ICoreScope ICoreScopeProvider.CreateCoreScope(
        IsolationLevel isolationLevel,
        RepositoryCacheMode repositoryCacheMode,
        IEventDispatcher? eventDispatcher,
        IScopedNotificationPublisher? scopedNotificationPublisher,
        bool? scopeFileSystems,
        bool callContext,
        bool autoComplete) =>
        CreateScope(
            isolationLevel,
            repositoryCacheMode,
            eventDispatcher,
            scopedNotificationPublisher,
            scopeFileSystems,
            callContext,
            autoComplete);

    /// <inheritdoc />
    IQuery<T> ICoreScopeProvider.CreateQuery<T>() =>
        SqlContext.Query<T>();

    /// <summary>
    ///     Creates an ambient scope.
    /// </summary>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <param name="repositoryCacheMode">The repositories cache mode.</param>
    /// <param name="eventDispatcher">An optional events dispatcher.</param>
    /// <param name="scopedNotificationPublisher">An optional notification publisher.</param>
    /// <param name="scopeFileSystems">A value indicating whether to scope the filesystems.</param>
    /// <param name="callContext">A value indicating whether this scope should always be registered in the call context.</param>
    /// <param name="autoComplete">A value indicating whether this scope is auto-completed.</param>
    /// <returns>The created ambient scope.</returns>
    /// <remarks>
    ///     <para>The created scope becomes the ambient scope.</para>
    ///     <para>If an ambient scope already exists, it becomes the parent of the created scope.</para>
    ///     <para>When the created scope is disposed, the parent scope becomes the ambient scope again.</para>
    ///     <para>Parameters must be specified on the outermost scope, or must be compatible with the parents.</para>
    ///     <para>
    ///         Auto-completed scopes should be used for read-only operations ONLY. Do not use them if you do not
    ///         understand the associated issues, such as the scope being completed even though an exception is thrown.
    ///     </para>
    /// </remarks>
    IScope CreateScope(
        IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        IEventDispatcher? eventDispatcher = null,
        IScopedNotificationPublisher? scopedNotificationPublisher = null,
        bool? scopeFileSystems = null,
        bool callContext = false,
        bool autoComplete = false);

    /// <summary>
    ///     Creates a detached scope.
    /// </summary>
    /// <returns>A detached scope.</returns>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <param name="repositoryCacheMode">The repositories cache mode.</param>
    /// <param name="eventDispatcher">An optional events dispatcher.</param>
    /// <param name="scopedNotificationPublisher">An option notification publisher.</param>
    /// <param name="scopeFileSystems">A value indicating whether to scope the filesystems.</param>
    /// <remarks>
    ///     <para>A detached scope is not ambient and has no parent.</para>
    ///     <para>It is meant to be attached by <see cref="AttachScope" />.</para>
    /// </remarks>
    /// <remarks>
    /// This is not used by CMS but is used by Umbraco Deploy.
    /// </remarks>
    IScope CreateDetachedScope(
        IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        IEventDispatcher? eventDispatcher = null,
        IScopedNotificationPublisher? scopedNotificationPublisher = null,
        bool? scopeFileSystems = null);

    /// <summary>
    ///     Attaches a scope.
    /// </summary>
    /// <param name="scope">The scope to attach.</param>
    /// <param name="callContext">A value indicating whether to force usage of call context.</param>
    /// <remarks>
    ///     <para>Only a scope created by <see cref="CreateDetachedScope" /> can be attached.</para>
    /// </remarks>
    void AttachScope(IScope scope, bool callContext = false);

    /// <summary>
    ///     Detaches a scope.
    /// </summary>
    /// <returns>The detached scope.</returns>
    /// <remarks>
    ///     <para>Only a scope previously attached by <see cref="AttachScope" /> can be detached.</para>
    /// </remarks>
    IScope DetachScope();

    /// <summary>
    ///     Gets the sql context.
    /// </summary>
    ISqlContext SqlContext { get; }

#if DEBUG_SCOPES
        IEnumerable<ScopeInfo> ScopeInfos { get; }
        ScopeInfo GetScopeInfo(IScope scope);
#endif
}

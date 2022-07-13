using System.Data;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Scoping;

/// <summary>
///     Provides scopes.
/// </summary>
public interface ICoreScopeProvider
{
    /// <summary>
    ///     Gets the scope context.
    /// </summary>
    IScopeContext? Context { get; }

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
    ICoreScope CreateCoreScope(
        IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        IEventDispatcher? eventDispatcher = null,
        IScopedNotificationPublisher? scopedNotificationPublisher = null,
        bool? scopeFileSystems = null,
        bool callContext = false,
        bool autoComplete = false);

    /// <summary>
    ///     Creates an instance of <see cref="IQuery{T}" />
    /// </summary>
    IQuery<T> CreateQuery<T>();
}

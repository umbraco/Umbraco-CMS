using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Provides functionality to create and manage EF Core scopes.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
public interface IEFCoreScopeProvider<TDbContext>
{
    /// <summary>
    /// Creates a new EF Core scope.
    /// </summary>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    /// <returns>The created scope.</returns>
    // TODO (V19): Change return type to IEFCoreScope<TDbContext> when IEfCoreScope<TDbContext> is removed.
#pragma warning disable CS0618 // Type or member is obsolete
    IEfCoreScope<TDbContext> CreateScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Creates a new detached EF Core scope that can be attached later.
    /// </summary>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    /// <returns>The created detached scope.</returns>
    // TODO (V19): Change return type to IEFCoreScope<TDbContext> when IEfCoreScope<TDbContext> is removed.
#pragma warning disable CS0618 // Type or member is obsolete
    IEfCoreScope<TDbContext> CreateDetachedScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Attaches a detached scope to the current ambient scope stack.
    /// </summary>
    /// <param name="other">The detached scope to attach.</param>
    // TODO (V19): Change parameter type to IEFCoreScope<TDbContext> when IEfCoreScope<TDbContext> is removed.
#pragma warning disable CS0618 // Type or member is obsolete
    void AttachScope(IEfCoreScope<TDbContext> other);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Detaches the current ambient scope from the stack.
    /// </summary>
    /// <returns>The detached scope.</returns>
    // TODO (V19): Change return type to IEFCoreScope<TDbContext> when IEfCoreScope<TDbContext> is removed.
#pragma warning disable CS0618 // Type or member is obsolete
    IEfCoreScope<TDbContext> DetachScope();
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Gets the ambient scope context.
    /// </summary>
    IScopeContext? AmbientScopeContext { get; }
}

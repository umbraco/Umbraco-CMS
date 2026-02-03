using Umbraco.Cms.Core.Scoping;
using CoreEFCoreScopeProvider = Umbraco.Cms.Core.Scoping.EFCore.IScopeProvider;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Provides functionality to create and manage EF Core scopes.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
public interface IEFCoreScopeProvider<TDbContext> : CoreEFCoreScopeProvider
{
    /// <summary>
    /// Creates a new EF Core scope.
    /// </summary>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    /// <returns>The created scope.</returns>
    new IEfCoreScope<TDbContext> CreateScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null);

    /// <summary>
    /// Creates a new detached EF Core scope that can be attached later.
    /// </summary>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    /// <returns>The created detached scope.</returns>
    new IEfCoreScope<TDbContext> CreateDetachedScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null);

    /// <summary>
    /// Attaches a detached scope to the current ambient scope stack.
    /// </summary>
    /// <param name="other">The detached scope to attach.</param>
    void AttachScope(IEfCoreScope<TDbContext> other);

    /// <summary>
    /// Detaches the current ambient scope from the stack.
    /// </summary>
    /// <returns>The detached scope.</returns>
    new IEfCoreScope<TDbContext> DetachScope();

    /// <summary>
    /// Gets the ambient scope context.
    /// </summary>
    new IScopeContext? AmbientScopeContext { get; }
}

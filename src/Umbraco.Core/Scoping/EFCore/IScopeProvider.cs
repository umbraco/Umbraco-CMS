namespace Umbraco.Cms.Core.Scoping.EFCore;

/// <summary>
/// Provides functionality to create and manage EF Core scopes without exposing DbContext.
/// </summary>
/// <remarks>
/// This is the abstract interface for use in Core. The concrete implementation
/// <c>IEFCoreScopeProvider&lt;TDbContext&gt;</c> in the EFCore persistence project
/// extends this interface and provides DbContext-aware methods.
/// </remarks>
public interface IScopeProvider
{
    /// <summary>
    /// Creates a new EF Core scope.
    /// </summary>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    /// <returns>The created scope as <see cref="ICoreScope"/>.</returns>
    ICoreScope CreateScope(
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null);

    /// <summary>
    /// Creates a new detached EF Core scope that can be attached later.
    /// </summary>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    /// <returns>The created detached scope as <see cref="ICoreScope"/>.</returns>
    ICoreScope CreateDetachedScope(
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null);

    /// <summary>
    /// Attaches a detached scope to the current ambient scope stack.
    /// </summary>
    /// <param name="other">The detached scope to attach.</param>
    void AttachScope(ICoreScope other);

    /// <summary>
    /// Detaches the current ambient scope from the stack.
    /// </summary>
    /// <returns>The detached scope.</returns>
    ICoreScope DetachScope();

    /// <summary>
    /// Gets the ambient scope context.
    /// </summary>
    IScopeContext? AmbientScopeContext { get; }
}

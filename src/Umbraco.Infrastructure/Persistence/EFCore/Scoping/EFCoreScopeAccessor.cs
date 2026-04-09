using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using CoreEFCoreScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

/// <summary>
/// Provides access to the current ambient EF Core scope.
/// When no EF Core scope exists but an NPoco scope is active, automatically creates
/// an EF Core bridge scope to allow EF Core repositories to participate in the NPoco transaction.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
internal sealed class EFCoreScopeAccessor<TDbContext> : IEFCoreScopeAccessor<TDbContext> where TDbContext : DbContext
{
    private readonly IAmbientEFCoreScopeStack<TDbContext> _ambientEfCoreScopeStack;

    // TODO: Remove bridge scope support when NPoco migration is complete
    private readonly IAmbientScopeStack _ambientScopeStack;
    private readonly Lazy<IEFCoreScopeProvider<TDbContext>> _efCoreScopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreScopeAccessor{TDbContext}"/> class.
    /// </summary>
    /// <param name="ambientEfCoreScopeStack">The ambient scope stack.</param>
    /// <param name="ambientScopeStack">The NPoco ambient scope stack, used to detect active NPoco scopes.</param>
    /// <param name="efCoreScopeProvider">
    /// Lazy provider to avoid circular dependencies (EFCoreScopeProvider depends on EFCoreScopeAccessor).
    /// Used to auto-create EF Core scopes when only an NPoco scope exists.
    /// </param>
    public EFCoreScopeAccessor(
        IAmbientEFCoreScopeStack<TDbContext> ambientEfCoreScopeStack,
        IAmbientScopeStack ambientScopeStack,
        Lazy<IEFCoreScopeProvider<TDbContext>> efCoreScopeProvider)
    {
        _ambientEfCoreScopeStack = ambientEfCoreScopeStack;
        _ambientScopeStack = ambientScopeStack;
        _efCoreScopeProvider = efCoreScopeProvider;
    }

    /// <summary>
    /// Gets the current ambient scope as a concrete EFCoreScope instance.
    /// </summary>
    public EFCoreScope<TDbContext>? AmbientScope => (EFCoreScope<TDbContext>?)GetOrCreateAmbientScope();

    /// <inheritdoc />
    public bool HasBridgedAmbientScope
        => _ambientEfCoreScopeStack.AmbientScope is not EFCoreScope<TDbContext> { IsBridgeScope: false };

    /// <inheritdoc />
    IEfCoreScope<TDbContext>? IEFCoreScopeAccessor<TDbContext>.AmbientScope => GetOrCreateAmbientScope();

    /// <inheritdoc />
    ICoreScope? CoreEFCoreScopeAccessor.AmbientScope => GetOrCreateAmbientScope();

    /// <summary>
    /// Returns the existing ambient EF Core scope, or creates a bridge scope if an NPoco scope is active.
    /// Bridge scopes are automatically cleaned up via the NPoco scope context's Enlist mechanism,
    /// so no manual stale-scope detection is needed here.
    ///
    /// This is temporary and should be removed when all repositories are migrated to EF Core.
    /// </summary>
    private IEfCoreScope<TDbContext>? GetOrCreateAmbientScope()
    {
        IEfCoreScope<TDbContext>? scope = _ambientEfCoreScopeStack.AmbientScope;

        if (scope is not null)
        {
            return scope;
        }

        var provider = (EFCoreScopeProvider<TDbContext>)_efCoreScopeProvider.Value;

        if (provider.ScopeContextDepth > 0)
        {
            return null;
        }

        // No EF Core scope on the stack. If an NPoco scope exists create a bridge scope
        if (_ambientScopeStack.AmbientScope is IScope npocoScope)
        {
            return provider.CreateBridgeScope(npocoScope);
        }

        return null;
    }
}

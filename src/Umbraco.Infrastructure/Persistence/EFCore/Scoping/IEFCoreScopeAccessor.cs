using CoreEFCoreScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

/// <summary>
/// Provides access to the current ambient EF Core scope.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
public interface IEFCoreScopeAccessor<TDbContext> : CoreEFCoreScopeAccessor
{
    /// <summary>
    ///     Gets the ambient scope.
    /// </summary>
    /// <remarks>
    ///     Returns <c>null</c> if there is no ambient scope. May auto-create a bridge scope
    ///     when only an NPoco scope is active; use <see cref="HasNonBridgeAmbientScope"/> to
    ///     check for a genuine EF Core scope without side effects.
    /// </remarks>
    new IEfCoreScope<TDbContext>? AmbientScope { get; }

    /// <summary>
    ///     Gets a value indicating whether there is no genuine (non-bridge) EF Core ambient scope on the stack.
    /// </summary>
    /// <remarks>
    ///     Returns <c>true</c> when there is no ambient scope at all, or when the only ambient scope is a
    ///     bridge scope auto-created from an NPoco scope. Returns <c>false</c> only when a real, explicitly
    ///     opened EF Core scope is active. Unlike <see cref="AmbientScope"/>, this property never creates a
    ///     bridge scope as a side effect. Use <c>!HasBridgedAmbientScope</c> in <see cref="IDistributedLockingMechanism.Enabled"/>
    ///     to avoid perturbing the scope stack during mechanism selection.
    /// </remarks>
    bool HasBridgedAmbientScope => true;
}

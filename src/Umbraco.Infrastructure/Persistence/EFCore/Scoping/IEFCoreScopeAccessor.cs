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
    /// <remarks>Returns <c>null</c> if there is no ambient scope.</remarks>
    new IEfCoreScope<TDbContext>? AmbientScope { get; }
}

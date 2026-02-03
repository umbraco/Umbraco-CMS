namespace Umbraco.Cms.Core.Scoping.EFCore;

/// <summary>
/// Provides access to the current ambient EF Core scope without exposing DbContext.
/// </summary>
/// <remarks>
/// This is the abstract interface for use in Core. The concrete implementation
/// <c>IEFCoreScopeAccessor&lt;TDbContext&gt;</c> in the EFCore persistence project
/// extends this interface and provides DbContext-aware access.
/// </remarks>
public interface IScopeAccessor
{
    /// <summary>
    /// Gets the ambient scope.
    /// </summary>
    /// <remarks>Returns <c>null</c> if there is no ambient scope.</remarks>
    ICoreScope? AmbientScope { get; }
}

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Provides access to the current ambient EF Core scope.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
public interface IEFCoreScopeAccessor<TDbContext>
{
    /// <summary>
    ///     Gets the ambient scope.
    /// </summary>
    /// <remarks>Returns <c>null</c> if there is no ambient scope.</remarks>
    // TODO (V19): Change return type to IEFCoreScope<TDbContext> when IEfCoreScope<TDbContext> is removed.
#pragma warning disable CS0618 // Type or member is obsolete
    IEfCoreScope<TDbContext>? AmbientScope { get; }
#pragma warning restore CS0618 // Type or member is obsolete
}

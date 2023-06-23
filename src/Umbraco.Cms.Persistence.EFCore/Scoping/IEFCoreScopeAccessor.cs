namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEFCoreScopeAccessor<TDbContext>
{
    /// <summary>
    ///     Gets the ambient scope.
    /// </summary>
    /// <remarks>Returns <c>null</c> if there is no ambient scope.</remarks>
    IEfCoreScope<TDbContext>? AmbientScope { get; }
}

namespace Umbraco.Cms.Infrastructure.Scoping;

public interface IScopeAccessor
{
    /// <summary>
    ///     Gets the ambient scope.
    /// </summary>
    /// <remarks>Returns <c>null</c> if there is no ambient scope.</remarks>
    IScope? AmbientScope { get; }
}

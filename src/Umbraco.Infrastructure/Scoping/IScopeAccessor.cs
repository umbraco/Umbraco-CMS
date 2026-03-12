namespace Umbraco.Cms.Infrastructure.Scoping;

/// <summary>
/// Defines a mechanism to access the current <see cref="IScope"/>, which manages the unit of work and transactions within the application.
/// </summary>
public interface IScopeAccessor
{
    /// <summary>
    ///     Gets the ambient scope.
    /// </summary>
    /// <remarks>Returns <c>null</c> if there is no ambient scope.</remarks>
    IScope? AmbientScope { get; }
}

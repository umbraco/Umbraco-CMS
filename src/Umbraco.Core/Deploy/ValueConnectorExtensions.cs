using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Extension methods adding backwards-compatability between <see cref="IValueConnector" /> and <see cref="IValueConnector2" />.
/// </summary>
/// <remarks>
/// These extension methods will be removed in Umbraco 13.
/// </remarks>
public static class ValueConnectorExtensions
{
    /// <summary>
    /// Gets the artifact value corresponding to a property value and gather dependencies.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="value">The property value.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="dependencies">The dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The artifact value.
    /// </returns>
    /// <remarks>
    /// This extension method tries to make use of the <see cref="IContextCache" /> on types also implementing <see cref="IValueConnector2" />.
    /// </remarks>
    public static string? ToArtifact(this IValueConnector connector, object? value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
        => connector is IValueConnector2 connector2
            ? connector2.ToArtifact(value, propertyType, dependencies, contextCache)
            : connector.ToArtifact(value, propertyType, dependencies);

    /// <summary>
    /// Gets the property value corresponding to an artifact value.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="value">The artifact value.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="currentValue">The current property value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The property value.
    /// </returns>
    /// <remarks>
    /// This extension method tries to make use of the <see cref="IContextCache" /> on types also implementing <see cref="IValueConnector2" />.
    /// </remarks>
    public static object? FromArtifact(this IValueConnector connector, string? value, IPropertyType propertyType, object? currentValue, IContextCache contextCache)
        => connector is IValueConnector2 connector2
            ? connector2.FromArtifact(value, propertyType, currentValue, contextCache)
            : connector.FromArtifact(value, propertyType, currentValue);
}

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Defines methods that can convert a property value to / from an environment-agnostic string.
/// </summary>
/// <remarks>
///     Property values may contain values such as content identifiers, that would be local
///     to one environment, and need to be converted in order to be deployed. Connectors also deal
///     with serializing to / from string.
/// </remarks>
public interface IValueConnector
{
    /// <summary>
    ///     Gets the property editor aliases that the value converter supports by default.
    /// </summary>
    IEnumerable<string> PropertyEditorAliases { get; }

    /// <summary>
    ///     Gets the deploy property value corresponding to a content property value, and gather dependencies.
    /// </summary>
    /// <param name="value">The content property value.</param>
    /// <param name="propertyType">The value property type</param>
    /// <param name="dependencies">The content dependencies.</param>
    /// <returns>The deploy property value.</returns>
    string? ToArtifact(object? value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies);

    /// <summary>
    ///     Gets the content property value corresponding to a deploy property value.
    /// </summary>
    /// <param name="value">The deploy property value.</param>
    /// <param name="propertyType">The value property type</param>
    /// <param name="currentValue">The current content property value.</param>
    /// <returns>The content property value.</returns>
    object? FromArtifact(string? value, IPropertyType propertyType, object? currentValue);
}

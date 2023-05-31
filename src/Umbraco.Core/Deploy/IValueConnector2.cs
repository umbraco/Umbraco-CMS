using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <inheritdoc />
/// <remarks>
/// This interface will be merged back into <see cref="IValueConnector" /> and removed in Umbraco 13.
/// </remarks>
public interface IValueConnector2 : IValueConnector
{
    /// <inheritdoc />
    [Obsolete($"Use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    string? IValueConnector.ToArtifact(object? value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        => ToArtifact(value, propertyType, dependencies, PassThroughCache.Instance);

    /// <summary>
    /// Gets the deploy property value corresponding to a content property value, and gather dependencies.
    /// </summary>
    /// <param name="value">The content property value.</param>
    /// <param name="propertyType">The value property type</param>
    /// <param name="dependencies">The content dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The deploy property value.
    /// </returns>
    string? ToArtifact(object? value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache);

    /// <inheritdoc />
    [Obsolete($"Use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    object? IValueConnector.FromArtifact(string? value, IPropertyType propertyType, object? currentValue)
        => FromArtifact(value, propertyType, currentValue, PassThroughCache.Instance);

    /// <summary>
    /// Gets the content property value corresponding to a deploy property value.
    /// </summary>
    /// <param name="value">The deploy property value.</param>
    /// <param name="propertyType">The value property type</param>
    /// <param name="currentValue">The current content property value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The content property value.
    /// </returns>
    object? FromArtifact(string? value, IPropertyType propertyType, object? currentValue, IContextCache contextCache);
}

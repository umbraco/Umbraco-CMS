using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a property on a content item.
/// </summary>
public interface IProperty : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets the value storage type for this property.
    /// </summary>
    ValueStorageType ValueStorageType { get; }

    /// <summary>
    ///     Returns the PropertyType, which this Property is based on
    /// </summary>
    IPropertyType PropertyType { get; }

    /// <summary>
    ///     Gets the list of values.
    /// </summary>
    IReadOnlyCollection<IPropertyValue> Values { get; set; }

    /// <summary>
    ///     Returns the Alias of the PropertyType, which this Property is based on
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets the identifier of the property type.
    /// </summary>
    int PropertyTypeId { get; }

    /// <summary>
    ///     Gets the value.
    /// </summary>
    object? GetValue(string? culture = null, string? segment = null, bool published = false);

    /// <summary>
    ///     Sets a value.
    /// </summary>
    /// <returns>true if the value was set (updated), false otherwise.</returns>
    /// <remarks>
    /// A false return value does not indicate failure, but rather that the property value was not changed
    /// (i.e. the value passed in was equal to the current property value).
    /// </remarks>
    bool SetValue(object? value, string? culture = null, string? segment = null);

    /// <summary>
    ///     Publishes property values for the specified culture and segment.
    /// </summary>
    /// <param name="culture">The culture to publish, or "*" for all cultures.</param>
    /// <param name="segment">The segment to publish, or "*" for all segments.</param>
    void PublishValues(string? culture = "*", string segment = "*");

    /// <summary>
    ///     Publishes partial property values for a specific data editor and culture.
    /// </summary>
    /// <param name="dataEditor">The data editor handling the partial publish.</param>
    /// <param name="culture">The culture to publish.</param>
    void PublishPartialValues(IDataEditor dataEditor, string? culture);

    /// <summary>
    ///     Unpublishes property values for the specified culture and segment.
    /// </summary>
    /// <param name="culture">The culture to unpublish, or "*" for all cultures.</param>
    /// <param name="segment">The segment to unpublish, or "*" for all segments.</param>
    void UnpublishValues(string? culture = "*", string segment = "*");
}

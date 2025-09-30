using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models;

public interface IProperty : IEntity, IRememberBeingDirty
{
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

    void PublishValues(string? culture = "*", string segment = "*");

    void PublishPartialValues(IDataEditor dataEditor, string? culture);

    void UnpublishValues(string? culture = "*", string segment = "*");
}

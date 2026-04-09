using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a property value within a block item.
/// </summary>
public sealed class BlockPropertyValue : ValueModelBase
{
    /// <summary>
    ///     Gets or sets the property type.
    /// </summary>
    /// <remarks>
    ///     Used during deserialization to populate the property value/property type of a block item content property.
    /// </remarks>
    [JsonIgnore]
    public IPropertyType? PropertyType { get; set; }

    /// <summary>
    ///     Gets the property editor alias.
    /// </summary>
    /// <value>
    ///     The property editor alias.
    /// </value>
    public string? EditorAlias => PropertyType?.PropertyEditorAlias;
}

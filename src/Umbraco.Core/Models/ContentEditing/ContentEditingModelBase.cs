namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the base model for content editing operations.
/// </summary>
public abstract class ContentEditingModelBase
{
    /// <summary>
    ///     Gets or sets the collection of property values for the content.
    /// </summary>
    public IEnumerable<PropertyValueModel> Properties { get; set; } = Array.Empty<PropertyValueModel>();

    /// <summary>
    ///     Gets or sets the collection of variant models for culture and segment variations.
    /// </summary>
    public IEnumerable<VariantModel> Variants { get; set; } = Array.Empty<VariantModel>();
}

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the base model for content with values and variants.
/// </summary>
/// <typeparam name="TValueModel">The type of value model, which must inherit from <see cref="ValueModelBase"/>.</typeparam>
/// <typeparam name="TVariantModel">The type of variant model, which must inherit from <see cref="VariantModelBase"/>.</typeparam>
public abstract class ContentModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    /// <summary>
    ///     Gets or sets the collection of property values for the content.
    /// </summary>
    public IEnumerable<TValueModel> Values { get; set; } = Enumerable.Empty<TValueModel>();

    /// <summary>
    ///     Gets or sets the collection of variant models for culture and segment variations.
    /// </summary>
    public IEnumerable<TVariantModel> Variants { get; set; } = Enumerable.Empty<TVariantModel>();
}

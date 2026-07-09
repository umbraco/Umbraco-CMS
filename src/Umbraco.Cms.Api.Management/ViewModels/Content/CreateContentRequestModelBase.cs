using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Serves as the base class for content creation request models, parameterized by value and variant model types.
/// </summary>
/// <typeparam name="TValueModel">Specifies the type representing the content's values.</typeparam>
/// <typeparam name="TVariantModel">Specifies the type representing the content's variants (e.g., language or segment variations).</typeparam>
public abstract class CreateContentRequestModelBase<TValueModel, TVariantModel>
    : ContentModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the content.
    /// </summary>
    public Guid? Id { get; set; }
}

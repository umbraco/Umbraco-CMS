using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

    /// <summary>
    /// Provides a base class for request models used to create content items with a specified parent in the content tree.
    /// </summary>
    /// <typeparam name="TValueModel">The type representing the value model for the content.</typeparam>
    /// <typeparam name="TVariantModel">The type representing the variant model for the content.</typeparam>
public abstract class CreateContentWithParentRequestModelBase<TValueModel, TVariantModel>
    : CreateContentRequestModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    /// <summary>
    /// Gets or sets the model representing the parent content by its ID.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }
}

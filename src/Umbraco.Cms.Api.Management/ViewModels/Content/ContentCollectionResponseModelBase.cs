using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Serves as the base response model for a collection of content items, parameterized by value and variant response model types.
/// </summary>
public abstract class ContentCollectionResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueResponseModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    /// <summary>
    /// Gets or sets the name or identifier of the user who created the content.
    /// </summary>
    public string? Creator { get; set; }

    /// <summary>
    /// Gets or sets the sort order of the content item within the collection.
    /// </summary>
    public int SortOrder { get; set; }
}

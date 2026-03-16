using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

/// <summary>
/// Represents the model used to create a new media item.
/// </summary>
public class CreateMediaRequestModel : CreateContentWithParentRequestModelBase<MediaValueModel, MediaVariantRequestModel>
{
    /// <summary>
    /// Gets or sets a reference to the media type, identified by its ID.
    /// </summary>
    public required ReferenceByIdModel MediaType { get; set; }
}

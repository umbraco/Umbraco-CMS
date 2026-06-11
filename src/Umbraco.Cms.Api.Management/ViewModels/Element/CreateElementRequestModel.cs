using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the API request model used for creating a new element in Umbraco.
/// </summary>
public class CreateElementRequestModel : CreateContentWithParentRequestModelBase<ElementValueModel, ElementVariantRequestModel>
{
    /// <summary>
    /// Gets or sets a reference to the document type for the element.
    /// </summary>
    public required ReferenceByIdModel DocumentType { get; set; }
}

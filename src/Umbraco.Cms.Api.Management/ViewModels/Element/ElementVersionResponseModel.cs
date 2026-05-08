namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents a response model containing information about a specific version of an element in the Umbraco CMS Management API.
/// </summary>
public class ElementVersionResponseModel : ElementResponseModelBase<ElementValueResponseModel, ElementVariantResponseModel>
{
    /// <summary>
    /// Gets or sets the reference to the element associated with this version.
    /// </summary>
    public ReferenceByIdModel? Element { get; set; }
}

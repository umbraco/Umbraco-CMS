namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents a response model for an element returned by the Umbraco CMS Management API.
/// </summary>
public class ElementResponseModel : ElementResponseModelBase<ElementValueResponseModel, ElementVariantResponseModel>
{
    /// <summary>
    /// Gets or sets a value indicating whether the element is trashed.
    /// </summary>
    public bool IsTrashed { get; set; }
}

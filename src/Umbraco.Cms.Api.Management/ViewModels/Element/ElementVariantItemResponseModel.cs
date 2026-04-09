using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents a variant item response model for an element, including its publish state.
/// </summary>
public class ElementVariantItemResponseModel : VariantItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the publish state of the element variant.
    /// </summary>
    public required DocumentVariantState State { get; set; }
}

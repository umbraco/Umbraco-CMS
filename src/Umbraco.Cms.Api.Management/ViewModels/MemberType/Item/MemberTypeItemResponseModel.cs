using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;

/// <summary>
/// Represents a response model containing information about a member type item.
/// </summary>
public class MemberTypeItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>Gets or sets the icon associated with the member type.</summary>
    public string? Icon { get; set; }
}

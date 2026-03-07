using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

/// <summary>
/// Represents a model used to return public access settings in API responses.
/// </summary>
public class PublicAccessResponseModel : PublicAccessBaseModel
{
    /// <summary>
    /// Gets or sets the collection of members associated with the public access response.
    /// </summary>
    public MemberItemResponseModel[] Members { get; set; } = Array.Empty<MemberItemResponseModel>();

    /// <summary>
    /// Gets or sets the collection of member group items associated with the public access response.
    /// </summary>
    public MemberGroupItemResponseModel[] Groups { get; set; } = Array.Empty<MemberGroupItemResponseModel>();
}

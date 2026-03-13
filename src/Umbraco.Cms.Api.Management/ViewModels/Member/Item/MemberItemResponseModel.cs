using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.Member.Item;

/// <summary>
/// Represents a response model containing details about a member item returned by the Umbraco Management API.
/// </summary>
public class MemberItemResponseModel : ItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the reference to the member type associated with this member.
    /// </summary>
    public MemberTypeReferenceResponseModel MemberType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variant items for the member.
    /// </summary>
    public IEnumerable<VariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<VariantItemResponseModel>();

    /// <summary>
    /// Gets or sets the type of member represented by this model.
    /// </summary>
    public MemberKind Kind { get; set; }
}

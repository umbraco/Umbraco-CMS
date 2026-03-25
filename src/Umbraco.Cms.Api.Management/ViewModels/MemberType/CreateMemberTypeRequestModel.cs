using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

/// <summary>
/// Represents the request model for creating a new member type.
/// </summary>
public class CreateMemberTypeRequestModel
    : CreateContentTypeWithParentRequestModelBase<CreateMemberTypePropertyTypeRequestModel, CreateMemberTypePropertyTypeContainerRequestModel>
{
    /// <summary>
    /// Gets or sets the compositions associated with the member type.
    /// </summary>
    public IEnumerable<MemberTypeComposition> Compositions { get; set; } = Enumerable.Empty<MemberTypeComposition>();
}

using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

/// <summary>
/// Represents the data required to update an existing member type in the system.
/// </summary>
public class UpdateMemberTypeRequestModel
    : UpdateContentTypeRequestModelBase<UpdateMemberTypePropertyTypeRequestModel, UpdateMemberTypePropertyTypeContainerRequestModel>
{
    /// <summary>
    /// Gets or sets the collection of compositions that are associated with this member type.
    /// </summary>
    public IEnumerable<MemberTypeComposition> Compositions { get; set; } = Enumerable.Empty<MemberTypeComposition>();
}

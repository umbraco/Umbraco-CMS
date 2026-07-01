namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents the response model containing information about a property type reference within a member type.
/// </summary>
public class MemberTypePropertyTypeReferenceResponseModel : ContentTypePropertyTypeReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the member type associated with this property type reference.
    /// </summary>
    public TrackedReferenceMemberType MemberType { get; set; } = new();
}

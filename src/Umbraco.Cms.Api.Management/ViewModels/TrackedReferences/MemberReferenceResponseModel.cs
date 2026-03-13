namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents a response model containing information about a member reference in tracked references.
/// </summary>
public class MemberReferenceResponseModel : ReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the type of the tracked reference member.
    /// </summary>
     public TrackedReferenceMemberType MemberType { get; set; } = new();
}

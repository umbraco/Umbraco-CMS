namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

/// <summary>
/// A request model for copying a member type.
/// </summary>
public class CopyMemberTypeRequestModel
{
    /// <summary>
    /// Gets or sets the target location, identified by ID, where the member type will be copied.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}

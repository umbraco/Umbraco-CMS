namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

/// <summary>
/// Request model used to move a member type.
/// </summary>
public class MoveMemberTypeRequestModel
{
    /// <summary>
    /// Gets or sets the target reference by ID model for the member type move operation.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}

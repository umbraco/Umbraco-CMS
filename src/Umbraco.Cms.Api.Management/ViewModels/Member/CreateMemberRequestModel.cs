using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Member;

/// <summary>
/// Represents a model for the data required to create a new member.
/// </summary>
public class CreateMemberRequestModel : CreateContentRequestModelBase<MemberValueModel, MemberVariantRequestModel>
{
    /// <summary>
    /// Gets or sets the email address of the member.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username for the member.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the member being created.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a reference to the member type to which the new member will belong.
    /// </summary>
    public required ReferenceByIdModel MemberType { get; set; }

    /// <summary>
    /// Gets or sets the collection of group IDs associated with the member.
    /// </summary>
    public IEnumerable<Guid>? Groups { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member is approved.
    /// </summary>
    public bool IsApproved { get; set; }
}

namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Request model for disabling a user.
/// </summary>
public class DisableUserRequestModel
{
    /// <summary>
    /// Gets or sets the collection of user IDs to be disabled.
    /// </summary>
    public HashSet<ReferenceByIdModel> UserIds { get; set; } = new();
}

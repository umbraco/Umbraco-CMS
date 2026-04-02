namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// A request model for unlocking user accounts.
/// </summary>
public class UnlockUsersRequestModel
{
    /// <summary>
    /// Gets or sets the set of user IDs to unlock.
    /// </summary>
    public ISet<ReferenceByIdModel> UserIds { get; set; } = new HashSet<ReferenceByIdModel>();
}

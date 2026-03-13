namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the request model used in the API for deleting one or more users.
/// Typically contains information identifying the users to be deleted.
/// </summary>
public class DeleteUsersRequestModel
{
    /// <summary>
    /// Gets or sets the collection of user IDs to be deleted.
    /// </summary>
    public HashSet<ReferenceByIdModel> UserIds { get; set; } = new();
}

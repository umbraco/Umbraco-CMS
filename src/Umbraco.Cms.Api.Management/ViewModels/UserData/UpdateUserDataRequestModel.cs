namespace Umbraco.Cms.Api.Management.ViewModels.UserData;

/// <summary>
/// Represents a request model containing the data required to update a user's profile or settings.
/// </summary>
public class UpdateUserDataRequestModel : UserDataViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public Guid Key { get; set; }
}

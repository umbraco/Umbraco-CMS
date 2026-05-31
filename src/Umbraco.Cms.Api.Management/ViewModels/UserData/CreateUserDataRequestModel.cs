namespace Umbraco.Cms.Api.Management.ViewModels.UserData;

/// <summary>
/// Represents the model for a request to create user data via the management API.
/// </summary>
public class CreateUserDataRequestModel : UserDataViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier key for the user data.
    /// </summary>
    public Guid? Key { get; set; }
}

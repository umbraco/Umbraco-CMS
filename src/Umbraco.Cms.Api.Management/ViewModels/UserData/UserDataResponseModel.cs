namespace Umbraco.Cms.Api.Management.ViewModels.UserData;

/// <summary>
/// Represents the response model containing user data returned by the Umbraco Management API.
/// </summary>
public class UserDataResponseModel : UserDataViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier key for the user data response.
    /// </summary>
    public Guid Key { get; set; }
}

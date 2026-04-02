namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Serves as the base view model for requests to create a new user in the system.
/// </summary>
public class CreateUserRequestModelBase : UserPresentationBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public Guid? Id { get; set; }
}

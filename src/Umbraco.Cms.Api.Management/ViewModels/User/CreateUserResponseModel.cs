namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the response model that contains information about a newly created user.
/// This model is returned by the API after a successful user creation operation.
/// </summary>
public class CreateUserResponseModel
{
    /// <summary>
    /// Gets or sets the reference to the created user by ID.
    /// </summary>
    public required ReferenceByIdModel User { get; set; }

    /// <summary>
    /// Gets or sets the initial password assigned to the user.
    /// </summary>
    public string? InitialPassword { get; set; }
}

namespace Umbraco.Cms.Api.Management.ViewModels.User.ClientCredentials;

/// <summary>
/// Represents a request model used in the API for creating client credentials for a user.
/// </summary>
public sealed class CreateUserClientCredentialsRequestModel
{
    /// <summary>
    /// Gets or sets the client identifier for the user client credentials.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret associated with the user client credentials for authentication purposes.
    /// </summary>
    public required string ClientSecret { get; set; }
}

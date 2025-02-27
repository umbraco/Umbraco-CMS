namespace Umbraco.Cms.Api.Management.ViewModels.User.ClientCredentials;

public sealed class CreateUserClientCredentialsRequestModel
{
    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }
}

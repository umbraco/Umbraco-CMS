namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class LoginRequestModel
{
    public required string Username { get; init; }

    public required string Password { get; init; }
}

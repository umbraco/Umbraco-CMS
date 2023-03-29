namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class CreateUserResponseModel
{
    public Guid UserKey { get; set; }

    public string? InitialPassword { get; set; }
}

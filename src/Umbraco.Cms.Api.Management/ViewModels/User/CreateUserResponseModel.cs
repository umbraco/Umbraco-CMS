namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class CreateUserResponseModel
{
    public Guid UserId { get; set; }

    public string? InitialPassword { get; set; }
}

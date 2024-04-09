namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class CreateUserResponseModel
{
    public required ReferenceByIdModel User { get; set; }

    public string? InitialPassword { get; set; }
}

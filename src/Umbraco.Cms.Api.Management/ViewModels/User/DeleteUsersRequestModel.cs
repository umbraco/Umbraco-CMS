namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class DeleteUsersRequestModel
{
    public HashSet<Guid> UserIds { get; set; } = new();
}

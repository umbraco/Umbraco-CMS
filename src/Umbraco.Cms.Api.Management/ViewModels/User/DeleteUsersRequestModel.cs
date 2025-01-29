namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class DeleteUsersRequestModel
{
    public HashSet<ReferenceByIdModel> UserIds { get; set; } = new();
}

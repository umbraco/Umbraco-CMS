namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UnlockUsersRequestModel
{
    public ISet<ReferenceByIdModel> UserIds { get; set; } = new HashSet<ReferenceByIdModel>();
}

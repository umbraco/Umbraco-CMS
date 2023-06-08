namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UnlockUsersRequestModel
{
    public ISet<Guid> UserIds { get; set; } = new HashSet<Guid>();
}

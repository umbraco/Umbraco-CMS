namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UserPresentationBase
{
    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public SortedSet<Guid> UserGroupIds { get; set; } = new();
}

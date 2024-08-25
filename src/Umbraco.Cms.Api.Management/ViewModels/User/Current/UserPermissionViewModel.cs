namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class UserPermissionViewModel
{
    public Guid NodeKey { get; set; }

    public ISet<string> Permissions { get; set; } = new HashSet<string>();
}

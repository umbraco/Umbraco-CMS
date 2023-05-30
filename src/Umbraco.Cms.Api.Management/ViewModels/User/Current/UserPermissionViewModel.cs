namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class UserPermissionViewModel
{
    public Guid NodeKey { get; set; }

    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
}

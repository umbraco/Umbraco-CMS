using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public class UserCreateModel
{
    public Guid? Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public UserType Type { get; set; }

    public ISet<Guid> UserGroupKeys { get; set; } = new HashSet<Guid>();
}

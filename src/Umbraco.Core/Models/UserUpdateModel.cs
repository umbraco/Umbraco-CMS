using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public class UserUpdateModel
{
    public required IUser ExistingUser { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public SortedSet<Guid> ContentStartNodeKeys { get; set; } = new();

    public SortedSet<Guid> MediaStartNodeKeys { get; set; } = new();

    public IEnumerable<IUserGroup> UserGroups { get; set; } = Enumerable.Empty<IUserGroup>();
}

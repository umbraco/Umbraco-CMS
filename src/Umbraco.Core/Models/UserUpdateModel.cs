namespace Umbraco.Cms.Core.Models;

public class UserUpdateModel
{
    public required Guid ExistingUserKey { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string LanguageIsoCode { get; set; } = string.Empty;

    public SortedSet<Guid> ContentStartNodeKeys { get; set; } = new();

    public SortedSet<Guid> MediaStartNodeKeys { get; set; } = new();

    public ISet<Guid> UserGroupKeys { get; set; } = new HashSet<Guid>();
}

namespace Umbraco.Cms.Core.Models;

public class UserUpdateModel
{
    public required Guid ExistingUserKey { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string LanguageIsoCode { get; set; } = string.Empty;

    public ISet<Guid> ContentStartNodeKeys { get; set; } = new HashSet<Guid>();

    public bool ContentRootAccess { get; set; }

    public ISet<Guid> MediaStartNodeKeys { get; set; } = new HashSet<Guid>();

    public bool MediaRootAccess { get; set; }

    public ISet<Guid> UserGroupKeys { get; set; } = new HashSet<Guid>();
}

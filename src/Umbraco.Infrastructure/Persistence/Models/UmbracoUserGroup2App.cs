namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoUserGroup2App
{
    public int UserGroupId { get; set; }

    public string App { get; set; } = null!;

    public virtual UmbracoUserGroup UserGroup { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoUserGroup
{
    public int Id { get; set; }

    public string UserGroupAlias { get; set; } = null!;

    public string UserGroupName { get; set; } = null!;

    public string? UserGroupDefaultPermissions { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public string? Icon { get; set; }

    public bool HasAccessToAllLanguages { get; set; }

    public int? StartContentId { get; set; }

    public int? StartMediaId { get; set; }

    public virtual UmbracoNode? StartContent { get; set; }

    public virtual UmbracoNode? StartMedia { get; set; }

    public virtual ICollection<UmbracoUserGroup2App> UmbracoUserGroup2Apps { get; set; } = new List<UmbracoUserGroup2App>();

    public virtual ICollection<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions { get; set; } = new List<UmbracoUserGroup2NodePermission>();

    public virtual ICollection<UmbracoLanguage> Languages { get; set; } = new List<UmbracoLanguage>();

    public virtual ICollection<UmbracoNode> Nodes { get; set; } = new List<UmbracoNode>();

    public virtual ICollection<UmbracoUser> Users { get; set; } = new List<UmbracoUser>();
}

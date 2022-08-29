using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoUserGroup
    {
        public UmbracoUserGroup()
        {
            UmbracoUserGroup2Apps = new HashSet<UmbracoUserGroup2App>();
            UmbracoUserGroup2NodePermissions = new HashSet<UmbracoUserGroup2NodePermission>();
            Languages = new HashSet<UmbracoLanguage>();
            Nodes = new HashSet<UmbracoNode>();
            Users = new HashSet<UmbracoUser>();
        }

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
        public virtual ICollection<UmbracoUserGroup2App> UmbracoUserGroup2Apps { get; set; }
        public virtual ICollection<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions { get; set; }

        public virtual ICollection<UmbracoLanguage> Languages { get; set; }
        public virtual ICollection<UmbracoNode> Nodes { get; set; }
        public virtual ICollection<UmbracoUser> Users { get; set; }
    }
}

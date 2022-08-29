using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoUserGroup2NodePermission
    {
        public int UserGroupId { get; set; }
        public int NodeId { get; set; }
        public string Permission { get; set; } = null!;

        public virtual UmbracoNode Node { get; set; } = null!;
        public virtual UmbracoUserGroup UserGroup { get; set; } = null!;
    }
}

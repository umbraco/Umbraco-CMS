using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoDataType
    {
        public UmbracoDataType()
        {
            CmsPropertyTypes = new HashSet<CmsPropertyType>();
        }

        public int NodeId { get; set; }
        public string PropertyEditorAlias { get; set; } = null!;
        public string DbType { get; set; } = null!;
        public string? Config { get; set; }

        public virtual UmbracoNode Node { get; set; } = null!;
        public virtual ICollection<CmsPropertyType> CmsPropertyTypes { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class CmsPropertyTypeGroup
    {
        public CmsPropertyTypeGroup()
        {
            CmsPropertyTypes = new HashSet<CmsPropertyType>();
        }

        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int ContenttypeNodeId { get; set; }
        public int Type { get; set; }
        public string Text { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public int Sortorder { get; set; }

        public virtual CmsContentType ContenttypeNode { get; set; } = null!;
        public virtual ICollection<CmsPropertyType> CmsPropertyTypes { get; set; }
    }
}

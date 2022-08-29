using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoRelationType
    {
        public UmbracoRelationType()
        {
            UmbracoRelations = new HashSet<UmbracoRelation>();
        }

        public int Id { get; set; }
        public Guid TypeUniqueId { get; set; }
        public bool Dual { get; set; }
        public Guid? ParentObjectType { get; set; }
        public Guid? ChildObjectType { get; set; }
        public string Name { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public bool? IsDependency { get; set; }

        public virtual ICollection<UmbracoRelation> UmbracoRelations { get; set; }
    }
}

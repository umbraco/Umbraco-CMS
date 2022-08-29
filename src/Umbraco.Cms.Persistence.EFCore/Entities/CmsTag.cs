using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class CmsTag
    {
        public CmsTag()
        {
            CmsTagRelationships = new HashSet<CmsTagRelationship>();
        }

        public int Id { get; set; }
        public string Group { get; set; } = null!;
        public int? LanguageId { get; set; }
        public string Tag { get; set; } = null!;

        public virtual UmbracoLanguage? Language { get; set; }
        public virtual ICollection<CmsTagRelationship> CmsTagRelationships { get; set; }
    }
}

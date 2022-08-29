using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoContent
    {
        public UmbracoContent()
        {
            CmsContentNus = new HashSet<CmsContentNu>();
            CmsTagRelationships = new HashSet<CmsTagRelationship>();
            UmbracoContentSchedules = new HashSet<UmbracoContentSchedule>();
            UmbracoContentVersions = new HashSet<UmbracoContentVersion>();
        }

        public int NodeId { get; set; }
        public int ContentTypeId { get; set; }

        public virtual CmsContentType ContentType { get; set; } = null!;
        public virtual UmbracoNode Node { get; set; } = null!;
        public virtual CmsMember? CmsMember { get; set; }
        public virtual UmbracoDocument? UmbracoDocument { get; set; }
        public virtual ICollection<CmsContentNu> CmsContentNus { get; set; }
        public virtual ICollection<CmsTagRelationship> CmsTagRelationships { get; set; }
        public virtual ICollection<UmbracoContentSchedule> UmbracoContentSchedules { get; set; }
        public virtual ICollection<UmbracoContentVersion> UmbracoContentVersions { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class CmsTemplate
    {
        public CmsTemplate()
        {
            CmsDocumentTypes = new HashSet<CmsDocumentType>();
            UmbracoDocumentVersions = new HashSet<UmbracoDocumentVersion>();
        }

        public int Pk { get; set; }
        public int NodeId { get; set; }
        public string? Alias { get; set; }

        public virtual UmbracoNode Node { get; set; } = null!;
        public virtual ICollection<CmsDocumentType> CmsDocumentTypes { get; set; }
        public virtual ICollection<UmbracoDocumentVersion> UmbracoDocumentVersions { get; set; }
    }
}

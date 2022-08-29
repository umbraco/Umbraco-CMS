using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class CmsDocumentType
    {
        public int ContentTypeNodeId { get; set; }
        public int TemplateNodeId { get; set; }
        public bool? IsDefault { get; set; }

        public virtual UmbracoNode ContentTypeNode { get; set; } = null!;
        public virtual CmsContentType ContentTypeNodeNavigation { get; set; } = null!;
        public virtual CmsTemplate TemplateNode { get; set; } = null!;
    }
}

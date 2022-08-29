using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class CmsContentType
    {
        public CmsContentType()
        {
            CmsContentTypeAllowedContentTypeAlloweds = new HashSet<CmsContentTypeAllowedContentType>();
            CmsContentTypeAllowedContentTypeIdNavigations = new HashSet<CmsContentTypeAllowedContentType>();
            CmsDocumentTypes = new HashSet<CmsDocumentType>();
            CmsMemberTypes = new HashSet<CmsMemberType>();
            CmsPropertyTypeGroups = new HashSet<CmsPropertyTypeGroup>();
            CmsPropertyTypes = new HashSet<CmsPropertyType>();
            UmbracoContents = new HashSet<UmbracoContent>();
        }

        public int Pk { get; set; }
        public int NodeId { get; set; }
        public string? Alias { get; set; }
        public string? Icon { get; set; }
        public string Thumbnail { get; set; } = null!;
        public string? Description { get; set; }
        public bool? IsContainer { get; set; }
        public bool? IsElement { get; set; }
        public bool? AllowAtRoot { get; set; }
        public int Variations { get; set; }

        public virtual UmbracoNode Node { get; set; } = null!;
        public virtual ICollection<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypeAlloweds { get; set; }
        public virtual ICollection<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypeIdNavigations { get; set; }
        public virtual ICollection<CmsDocumentType> CmsDocumentTypes { get; set; }
        public virtual ICollection<CmsMemberType> CmsMemberTypes { get; set; }
        public virtual ICollection<CmsPropertyTypeGroup> CmsPropertyTypeGroups { get; set; }
        public virtual ICollection<CmsPropertyType> CmsPropertyTypes { get; set; }
        public virtual ICollection<UmbracoContent> UmbracoContents { get; set; }
    }
}

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class CmsContentType
{
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

    public virtual ICollection<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypeAlloweds { get; set; } = new List<CmsContentTypeAllowedContentType>();

    public virtual ICollection<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypeIdNavigations { get; set; } = new List<CmsContentTypeAllowedContentType>();

    public virtual ICollection<CmsDocumentType> CmsDocumentTypes { get; set; } = new List<CmsDocumentType>();

    public virtual ICollection<CmsMemberType> CmsMemberTypes { get; set; } = new List<CmsMemberType>();

    public virtual ICollection<CmsPropertyTypeGroup> CmsPropertyTypeGroups { get; set; } = new List<CmsPropertyTypeGroup>();

    public virtual ICollection<CmsPropertyType> CmsPropertyTypes { get; set; } = new List<CmsPropertyType>();

    public virtual UmbracoNode Node { get; set; } = null!;

    public virtual UmbracoContentVersionCleanupPolicy? UmbracoContentVersionCleanupPolicy { get; set; }

    public virtual ICollection<UmbracoContent> UmbracoContents { get; set; } = new List<UmbracoContent>();
}

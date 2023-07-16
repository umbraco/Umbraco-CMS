namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoNode
{
    public int Id { get; set; }

    public Guid UniqueId { get; set; }

    public int ParentId { get; set; }

    public int Level { get; set; }

    public string Path { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool? Trashed { get; set; }

    public int? NodeUser { get; set; }

    public string? Text { get; set; }

    public Guid? NodeObjectType { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual CmsContentType? CmsContentType { get; set; }

    public virtual ICollection<CmsDocumentType> CmsDocumentTypes { get; set; } = new List<CmsDocumentType>();

    public virtual ICollection<CmsMemberType> CmsMemberTypes { get; set; } = new List<CmsMemberType>();

    public virtual CmsTemplate? CmsTemplate { get; set; }

    public virtual ICollection<UmbracoNode> InverseParent { get; set; } = new List<UmbracoNode>();

    public virtual UmbracoUser? NodeUserNavigation { get; set; }

    public virtual UmbracoNode Parent { get; set; } = null!;

    public virtual ICollection<UmbracoAccess> UmbracoAccessLoginNodes { get; set; } = new List<UmbracoAccess>();

    public virtual ICollection<UmbracoAccess> UmbracoAccessNoAccessNodes { get; set; } = new List<UmbracoAccess>();

    public virtual UmbracoAccess? UmbracoAccessNode { get; set; }

    public virtual UmbracoContent? UmbracoContent { get; set; }

    public virtual UmbracoDataType? UmbracoDataType { get; set; }

    public virtual ICollection<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations { get; set; } = new List<UmbracoDocumentCultureVariation>();

    public virtual ICollection<UmbracoDomain> UmbracoDomains { get; set; } = new List<UmbracoDomain>();

    public virtual ICollection<UmbracoRedirectUrl> UmbracoRedirectUrls { get; set; } = new List<UmbracoRedirectUrl>();

    public virtual ICollection<UmbracoRelation> UmbracoRelationChildren { get; set; } = new List<UmbracoRelation>();

    public virtual ICollection<UmbracoRelation> UmbracoRelationParents { get; set; } = new List<UmbracoRelation>();

    public virtual ICollection<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies { get; set; } = new List<UmbracoUser2NodeNotify>();

    public virtual ICollection<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions { get; set; } = new List<UmbracoUserGroup2NodePermission>();

    public virtual ICollection<UmbracoUserGroup> UmbracoUserGroupStartContents { get; set; } = new List<UmbracoUserGroup>();

    public virtual ICollection<UmbracoUserGroup> UmbracoUserGroupStartMedia { get; set; } = new List<UmbracoUserGroup>();

    public virtual ICollection<UmbracoUserStartNode> UmbracoUserStartNodes { get; set; } = new List<UmbracoUserStartNode>();

    public virtual ICollection<UmbracoNode> ChildContentTypes { get; set; } = new List<UmbracoNode>();

    public virtual ICollection<CmsMember> Members { get; set; } = new List<CmsMember>();

    public virtual ICollection<UmbracoNode> ParentContentTypes { get; set; } = new List<UmbracoNode>();

    public virtual ICollection<UmbracoUserGroup> UserGroups { get; set; } = new List<UmbracoUserGroup>();
}

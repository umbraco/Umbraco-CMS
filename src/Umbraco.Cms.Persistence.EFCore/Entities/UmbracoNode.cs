using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoNode
    {
        public UmbracoNode()
        {
            CmsDocumentTypes = new HashSet<CmsDocumentType>();
            CmsMemberTypes = new HashSet<CmsMemberType>();
            InverseParent = new HashSet<UmbracoNode>();
            UmbracoAccessLoginNodes = new HashSet<UmbracoAccess>();
            UmbracoAccessNoAccessNodes = new HashSet<UmbracoAccess>();
            UmbracoDocumentCultureVariations = new HashSet<UmbracoDocumentCultureVariation>();
            UmbracoDomains = new HashSet<UmbracoDomain>();
            UmbracoRedirectUrls = new HashSet<UmbracoRedirectUrl>();
            UmbracoRelationChildren = new HashSet<UmbracoRelation>();
            UmbracoRelationParents = new HashSet<UmbracoRelation>();
            UmbracoUser2NodeNotifies = new HashSet<UmbracoUser2NodeNotify>();
            UmbracoUserGroup2NodePermissions = new HashSet<UmbracoUserGroup2NodePermission>();
            UmbracoUserGroupStartContents = new HashSet<UmbracoUserGroup>();
            UmbracoUserGroupStartMedia = new HashSet<UmbracoUserGroup>();
            UmbracoUserStartNodes = new HashSet<UmbracoUserStartNode>();
            ChildContentTypes = new HashSet<UmbracoNode>();
            Members = new HashSet<CmsMember>();
            ParentContentTypes = new HashSet<UmbracoNode>();
            UserGroups = new HashSet<UmbracoUserGroup>();
        }

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

        public virtual UmbracoUser? NodeUserNavigation { get; set; }
        public virtual UmbracoNode Parent { get; set; } = null!;
        public virtual CmsContentType? CmsContentType { get; set; }
        public virtual CmsTemplate? CmsTemplate { get; set; }
        public virtual UmbracoAccess? UmbracoAccessNode { get; set; }
        public virtual UmbracoContent? UmbracoContent { get; set; }
        public virtual UmbracoDataType? UmbracoDataType { get; set; }
        public virtual ICollection<CmsDocumentType> CmsDocumentTypes { get; set; }
        public virtual ICollection<CmsMemberType> CmsMemberTypes { get; set; }
        public virtual ICollection<UmbracoNode> InverseParent { get; set; }
        public virtual ICollection<UmbracoAccess> UmbracoAccessLoginNodes { get; set; }
        public virtual ICollection<UmbracoAccess> UmbracoAccessNoAccessNodes { get; set; }
        public virtual ICollection<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations { get; set; }
        public virtual ICollection<UmbracoDomain> UmbracoDomains { get; set; }
        public virtual ICollection<UmbracoRedirectUrl> UmbracoRedirectUrls { get; set; }
        public virtual ICollection<UmbracoRelation> UmbracoRelationChildren { get; set; }
        public virtual ICollection<UmbracoRelation> UmbracoRelationParents { get; set; }
        public virtual ICollection<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies { get; set; }
        public virtual ICollection<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions { get; set; }
        public virtual ICollection<UmbracoUserGroup> UmbracoUserGroupStartContents { get; set; }
        public virtual ICollection<UmbracoUserGroup> UmbracoUserGroupStartMedia { get; set; }
        public virtual ICollection<UmbracoUserStartNode> UmbracoUserStartNodes { get; set; }

        public virtual ICollection<UmbracoNode> ChildContentTypes { get; set; }
        public virtual ICollection<CmsMember> Members { get; set; }
        public virtual ICollection<UmbracoNode> ParentContentTypes { get; set; }
        public virtual ICollection<UmbracoUserGroup> UserGroups { get; set; }
    }
}

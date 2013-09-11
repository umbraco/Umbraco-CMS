using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoNode")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MemberReadOnlyDto
    {
        /* from umbracoNode */
        [Column("id")]
        public int NodeId { get; set; }

        [Column("trashed")]
        public bool Trashed { get; set; }

        [Column("parentID")]
        public int ParentId { get; set; }

        [Column("nodeUser")]
        public int? UserId { get; set; }

        [Column("level")]
        public short Level { get; set; }

        [Column("path")]
        public string Path { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

        [Column("uniqueID")]
        public Guid? UniqueId { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("nodeObjectType")]
        public Guid? NodeObjectType { get; set; }

        [Column("createDate")]
        public DateTime CreateDate { get; set; }

        /* cmsContent */
        [Column("contentType")]
        public int ContentTypeId { get; set; }

        /* from cmsContentType joined with cmsContent */
        [Column("ContentTypeAlias")]
        public string ContentTypeAlias { get; set; }

        /* cmsContentVersion */
        [Column("VersionId")]
        public Guid VersionId { get; set; }

        [Column("VersionDate")]
        public DateTime UpdateDate { get; set; }

        [Column("LanguageLocale")]
        public string Language { get; set; }

        /*  cmsMember */
        [Column("Email")]
        public string Email { get; set; }

        [Column("LoginName")]
        public string LoginName { get; set; }

        [Column("Password")]
        public string Password { get; set; }

        /* Properties */
        [ResultColumn]
        public List<PropertyDataReadOnlyDto> Properties { get; set; }
    }
}
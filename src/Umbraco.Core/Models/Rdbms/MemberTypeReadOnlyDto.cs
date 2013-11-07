using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoNode")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MemberTypeReadOnlyDto
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

        /* cmsContentType */
        [Column("pk")]
        public int PrimaryKey { get; set; }

        [Column("alias")]
        public string Alias { get; set; }

        [Column("icon")]
        public string Icon { get; set; }

        [Column("thumbnail")]
        public string Thumbnail { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("isContainer")]
        public bool IsContainer { get; set; }

        [Column("allowAtRoot")]
        public bool AllowAtRoot { get; set; }

        /* PropertyTypes */
        //TODO Add PropertyTypeDto (+MemberTypeDto and DataTypeDto as one) ReadOnly list
        [ResultColumn]
        public List<PropertyTypeReadOnlyDto> PropertyTypes { get; set; }

        /* PropertyTypeGroups */
        //TODO Add PropertyTypeGroupDto ReadOnly list
        [ResultColumn]
        public List<PropertyTypeGroupReadOnlyDto> PropertyTypeGroups { get; set; }
    }
}
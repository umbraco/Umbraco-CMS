using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoNode")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class NodeDto
    {
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
    }
}
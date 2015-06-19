using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoNode")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class NodeDto
    {
        public const int NodeIdSeed = 1050;

        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_structure", IdentitySeed = NodeIdSeed)]
        public int NodeId { get; set; }

        [Column("trashed")]
        [Constraint(Default = "0")]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoNodeTrashed")]
        public bool Trashed { get; set; }

        [Column("parentID")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoNodeParentId")]
        public int ParentId { get; set; }

        [Column("nodeUser")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? UserId { get; set; }

        [Column("level")]
        public short Level { get; set; }

        [Column("path")]
        [Length(150)]
        public string Path { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

        [Column("uniqueID")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoNodeUniqueID")]
        public Guid UniqueId { get; set; }

        [Column("text")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Text { get; set; }

        [Column("nodeObjectType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoNodeObjectType")]
        public Guid? NodeObjectType { get; set; }

        [Column("createDate")]
        [Constraint(Default = "getdate()")]
        public DateTime CreateDate { get; set; }
    }
}
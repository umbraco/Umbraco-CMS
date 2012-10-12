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
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_structure")]
        [DatabaseType(DatabaseTypes.Integer)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int NodeId { get; set; }

        [Column("trashed")]
        [DatabaseType(DatabaseTypes.Bool)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = "0")]
        public bool Trashed { get; set; }

        [Column("parentID")]
        [DatabaseType(DatabaseTypes.Integer)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(NodeDto))]
        [IndexAttribute(IndexTypes.Nonclustered, Name = "IX_umbracoNodeParentId")]
        public int ParentId { get; set; }

        [Column("nodeUser")]
        [DatabaseType(DatabaseTypes.Integer)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? UserId { get; set; }

        [Column("level")]
        [DatabaseType(DatabaseTypes.SmallInteger)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public short Level { get; set; }

        [Column("path")]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 150)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Path { get; set; }

        [Column("sortOrder")]
        [DatabaseType(DatabaseTypes.Integer)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int SortOrder { get; set; }

        [Column("uniqueID")]
        [DatabaseType(DatabaseTypes.UniqueIdentifier)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? UniqueId { get; set; }

        [Column("text")]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 255)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Text { get; set; }

        [Column("nodeObjectType")]
        [DatabaseType(DatabaseTypes.UniqueIdentifier)]
        [NullSetting(NullSetting = NullSettings.Null)]
        [IndexAttribute(IndexTypes.Nonclustered, Name = "IX_umbracoNodeObjectType")]
        public Guid? NodeObjectType { get; set; }

        [Column("createDate")]
        [DatabaseType(DatabaseTypes.DateTime)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = "getdate()")]
        public DateTime CreateDate { get; set; }
    }
}
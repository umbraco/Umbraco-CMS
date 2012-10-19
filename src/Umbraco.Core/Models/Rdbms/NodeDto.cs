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
        //[DatabaseType(DatabaseTypes.Integer)]
        public int NodeId { get; set; }

        [Column("trashed")]
        //[DatabaseType(DatabaseTypes.Bool)]
        [Constraint(Default = "0")]
        public bool Trashed { get; set; }

        [Column("parentID")]
        //[DatabaseType(DatabaseTypes.Integer)]
        [ForeignKey(typeof(NodeDto))]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_umbracoNodeParentId")]
        public int ParentId { get; set; }

        [Column("nodeUser")]
        //[DatabaseType(DatabaseTypes.Integer)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? UserId { get; set; }

        [Column("level")]
        //[DatabaseType(DatabaseTypes.SmallInteger)]
        public short Level { get; set; }

        [Column("path")]
        [DatabaseType(SpecialDbTypes.NVARCHAR, Length = 150)]
        public string Path { get; set; }

        [Column("sortOrder")]
        //[DatabaseType(DatabaseTypes.Integer)]
        public int SortOrder { get; set; }

        [Column("uniqueID")]
        //[DatabaseType(DatabaseTypes.UniqueIdentifier)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? UniqueId { get; set; }

        [Column("text")]
        //[DatabaseType(DatabaseTypes.Nvarchar, Length = 255)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Text { get; set; }

        [Column("nodeObjectType")]
        //[DatabaseType(DatabaseTypes.UniqueIdentifier)]
        [NullSetting(NullSetting = NullSettings.Null)]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_umbracoNodeObjectType")]
        public Guid? NodeObjectType { get; set; }

        [Column("createDate")]
        //[DatabaseType(DatabaseTypes.DateTime)]
        [Constraint(Default = "getdate()")]
        public DateTime CreateDate { get; set; }
    }
}
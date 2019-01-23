using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class NodeDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.Node;
        public const int NodeIdSeed = 1060;
        private int? _userId;

        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
        public int NodeId { get; set; }

        [Column("uniqueId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_UniqueId")]
        [Constraint(Default = SystemMethods.NewGuid)]
        public Guid UniqueId { get; set; }

        [Column("parentId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_ParentId")]
        public int ParentId { get; set; }

        [Column("level")]
        public short Level { get; set; }

        [Column("path")]
        [Length(150)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Path")]
        public string Path { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

        [Column("trashed")]
        [Constraint(Default = "0")]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Trashed")]
        public bool Trashed { get; set; }

        [Column("nodeUser")] // todo: db rename to 'createUserId'
        [ForeignKey(typeof(UserDto))]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; } //return null if zero

        [Column("text")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Text { get; set; }

        [Column("nodeObjectType")] // todo: db rename to 'objectType'
        [NullSetting(NullSetting = NullSettings.Null)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_ObjectType")]
        public Guid? NodeObjectType { get; set; }

        [Column("createDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime CreateDate { get; set; }
    }
}

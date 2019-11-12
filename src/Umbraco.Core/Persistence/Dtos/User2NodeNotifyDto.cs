using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("userId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class User2NodeNotifyDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.User2NodeNotify;

        [Column("userId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodeNotify", OnColumns = "userId, nodeId, action")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_NodeId")]
        public int NodeId { get; set; }

        [Column("action")]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        [Length(1)]
        public string Action { get; set; }
    }
}

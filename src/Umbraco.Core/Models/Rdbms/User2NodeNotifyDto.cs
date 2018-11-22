using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser2NodeNotify")]
    [PrimaryKey("userId", autoIncrement = false)]
    [ExplicitColumns]
    internal class User2NodeNotifyDto
    {
        [Column("userId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodeNotify", OnColumns = "userId, nodeId, action")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("action")]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        [Length(1)]
        public string Action { get; set; }
    }
}
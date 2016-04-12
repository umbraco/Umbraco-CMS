using NPoco;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser2NodePermission")]
    [PrimaryKey("userId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class User2NodePermissionDto
    {
        [Column("userId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodePermission", OnColumns = "userId, nodeId, permission")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("permission")]
        public string Permission { get; set; }
    }
}
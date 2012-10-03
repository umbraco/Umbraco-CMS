using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser2NodePermission")]
    [PrimaryKey("userId", autoIncrement = false)]
    [ExplicitColumns]
    internal class User2NodePermissionDto
    {
        [Column("userId")]
        public int UserId { get; set; }

        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("permission")]
        public string Permission { get; set; }
    }
}
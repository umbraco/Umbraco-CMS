using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser2NodeNotify")]
    [PrimaryKey("userId", autoIncrement = false)]
    [ExplicitColumns]
    internal class User2NodeNotifyDto
    {
        [Column("userId")]
        public int UserId { get; set; }

        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("action")]
        public string Action { get; set; }
    }
}
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTagRelationship")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class TagRelationshipDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("tagId")]
        public int TagId { get; set; }
    }
}
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTagRelationship")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class TagRelationshipDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsTagRelationship", OnColumns = "nodeId, tagId")]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("tagId")]
        [ForeignKey(typeof(TagDto))]
        public int TagId { get; set; }
    }
}
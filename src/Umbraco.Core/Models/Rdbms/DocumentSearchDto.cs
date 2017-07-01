using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDocumentSearch")]
    [PrimaryKey("NodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class DocumentSearchDto
    {
        [Column("NodeId")]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("SearchText")]
        public string SearchText { get; set; }
    }
}
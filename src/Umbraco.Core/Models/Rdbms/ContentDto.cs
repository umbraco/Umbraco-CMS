using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class ContentDto
    {
        private const string TableName = Constants.DatabaseSchema.Tables.Content;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_NodeId")]
        public int NodeId { get; set; }

        [Column("contentTypeId")]
        [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
        public int ContentTypeId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "NodeId")]
        public NodeDto NodeDto { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")] // FIXME not one-to-one! BUT it depends on the query!
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}

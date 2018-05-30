using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("nodeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class ContentDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.Content;

        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("contentTypeId")]
        [ForeignKey(typeof(ContentTypeDto), Column = "NodeId")]
        public int ContentTypeId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "NodeId")]
        public NodeDto NodeDto { get; set; }

        // although a content has many content versions,
        // they can only be loaded one by one (as several content),
        // so this here is a OneToOne reference
        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}

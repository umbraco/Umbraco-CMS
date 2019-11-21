using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("nodeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class TagRelationshipDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.TagRelationship;

        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsTagRelationship", OnColumns = "nodeId, propertyTypeId, tagId")]
        [ForeignKey(typeof(ContentDto), Name = "FK_cmsTagRelationship_cmsContent", Column = "nodeId")]
        public int NodeId { get; set; }

        [Column("tagId")]
        [ForeignKey(typeof(TagDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_TagId")]
        public int TagId { get; set; }

        [Column("propertyTypeId")]
        [ForeignKey(typeof(PropertyTypeDto), Name = "FK_cmsTagRelationship_cmsPropertyType")]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_PropertyTypeId")]
        public int PropertyTypeId { get; set; }
    }
}

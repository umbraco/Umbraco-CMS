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
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsTagRelationship", OnColumns = "nodeId, propertyTypeId, tagId")]
        [ForeignKey(typeof(ContentDto), Name = "FK_cmsTagRelationship_cmsContent", Column = "nodeId")]
        public int NodeId { get; set; }

        [Column("tagId")]
        [ForeignKey(typeof(TagDto))]
        public int TagId { get; set; }

        [Column("propertyTypeId")]
        [ForeignKey(typeof(PropertyTypeDto), Name = "FK_cmsTagRelationship_cmsPropertyType")]
        public int PropertyTypeId { get; set; }
    }
}
using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyTypeGroup")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    internal class PropertyTypeGroupDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 12)]
        public int Id { get; set; }

        [Column("parentGroupId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        //[Constraint(Default = "NULL")]
        [ForeignKey(typeof(PropertyTypeGroupDto))]
        public int? ParentGroupId { get; set; }

        [Column("contenttypeNodeId")]
        [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
        public int ContentTypeNodeId { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("sortorder")]
        public int SortOrder { get; set; }

        [ResultColumn]
        public List<PropertyTypeDto> PropertyTypeDtos { get; set; }
    }
}
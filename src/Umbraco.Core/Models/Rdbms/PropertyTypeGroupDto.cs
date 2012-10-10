using System.Collections.Generic;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyTypeGroup")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    internal class PropertyTypeGroupDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("parentGroupId")]
        public int? ParentGroupId { get; set; }

        [Column("contenttypeNodeId")]
        public int ContentTypeNodeId { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("sortorder")]
        public int SortOrder { get; set; }

        [ResultColumn]
        public List<PropertyTypeDto> PropertyTypeDtos { get; set; }
    }
}
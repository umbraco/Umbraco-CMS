using System.Collections.Generic;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTab")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TabDto
    {
        [Column("id")]
        public int Id { get; set; }

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
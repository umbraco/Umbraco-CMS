using System.Collections.Generic;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class PropertyTypeDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("propertyTypeGroupId")]
        public int? TabId { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("helpText")]
        public string HelpText { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

        [Column("mandatory")]
        public bool Mandatory { get; set; }

        [Column("validationRegExp")]
        public string ValidationRegExp { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [ResultColumn]
        public DataTypeDto DataTypeDto { get; set; }
    }
}
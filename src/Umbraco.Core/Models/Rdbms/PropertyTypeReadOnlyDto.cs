using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class PropertyTypeReadOnlyDto
    {
        [Column("PropertyTypeId")]
        public int? Id { get; set; }

        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("PropertyTypesGroupId")]
        public int? PropertyTypeGroupId { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("PropertyTypeSortOrder")]
        public int SortOrder { get; set; }

        [Column("mandatory")]
        public bool Mandatory { get; set; }

        [Column("validationRegExp")]
        public string ValidationRegExp { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        /* cmsMemberType */
        [Column("memberCanEdit")]
        public bool CanEdit { get; set; }

        [Column("viewOnProfile")]
        public bool ViewOnProfile { get; set; }

        /* cmsDataType */
        [Column("propertyEditorAlias")]
        public string PropertyEditorAlias { get; set; }

        [Column("dbType")]
        public string DbType { get; set; }
    }
}
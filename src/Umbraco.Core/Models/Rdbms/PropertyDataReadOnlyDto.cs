using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyData")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    public class PropertyDataReadOnlyDto
    {
        /* cmsPropertyData */
        [Column("id")]
        public int Id { get; set; }

        [Column("contentNodeId")]
        public int NodeId { get; set; }

        [Column("VersionId")]
        public Guid? VersionId { get; set; }

        [Column("propertytypeid")]
        public int PropertyTypeId { get; set; }

        [Column("dataInt")]
        public int? Integer { get; set; }

        [Column("dataDate")]
        public DateTime? Date { get; set; }

        [Column("dataNvarchar")]
        public string VarChar { get; set; }

        [Column("dataNtext")]
        public string Text { get; set; }

        /* cmsPropertyType */
        [Column("dataTypeId")]
        public int DataTypeId { get; set; }

        [Column("propertyTypeGroupId")]
        public int? PropertyTypeGroupId { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("helpText")]
        public string HelpText { get; set; }

        [Column("mandatory")]
        public bool Mandatory { get; set; }

        [Column("validationRegExp")]
        public string ValidationRegExp { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        /* cmsDataType */
        [Column("controlId")]
        public Guid ControlId { get; set; }
    }
}
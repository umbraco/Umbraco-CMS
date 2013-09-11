using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class PropertyDataReadOnlyDto
    {
        /* cmsPropertyType */
        [Column("id")]
        public int Id { get; set; }

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

        [Column("PropertyTypeSortOrder")]
        public int SortOrder { get; set; }

        /* cmsDataType */
        [Column("propertyEditorAlias")]
        public string PropertyEditorAlias { get; set; }

        [Column("dbType")]
        public string DbType { get; set; }

        /* cmsPropertyData */
        [Column("PropertyDataId")]
        public int? PropertyDataId { get; set; }

        [Column("propertytypeid")]
        public int? PropertyTypeId { get; set; }

        [Column("VersionId")]
        public Guid VersionId { get; set; }

        [Column("dataInt")]
        public int? Integer { get; set; }

        [Column("dataDate")]
        public DateTime? Date { get; set; }

        [Column("dataNvarchar")]
        public string VarChar { get; set; }

        [Column("dataNtext")]
        public string Text { get; set; }

        [Ignore]
        public object GetValue
        {
            get
            {
                if (Integer.HasValue)
                {
                    return Integer.Value;
                }

                if (Date.HasValue)
                {
                    return Date.Value;
                }

                if (string.IsNullOrEmpty(VarChar) == false)
                {
                    return VarChar;
                }

                if (string.IsNullOrEmpty(Text) == false)
                {
                    return Text;
                }

                return string.Empty;
            }
        }
    }
}
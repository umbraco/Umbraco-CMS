using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class PropertyTypeDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 50)]
        public int Id { get; set; }

        [Column("dataTypeId")]
        [ForeignKey(typeof(DataTypeDto), Column = "nodeId")]
        public int DataTypeId { get; set; }

        [Column("contentTypeId")]
        [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
        public int ContentTypeId { get; set; }

        [Column("propertyTypeGroupId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(PropertyTypeGroupDto))]
        public int? PropertyTypeGroupId { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }

        [Column("Name")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Name { get; set; }

        [Column("sortOrder")]
        [Constraint(Default = "0")]
        public int SortOrder { get; set; }

        [Column("mandatory")]
        [Constraint(Default = "0")]
        public bool Mandatory { get; set; }

        [Column("validationRegExp")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ValidationRegExp { get; set; }

        [Column("Description")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(2000)]
        public string Description { get; set; }

        [ResultColumn]
        public DataTypeDto DataTypeDto { get; set; }
    }
}
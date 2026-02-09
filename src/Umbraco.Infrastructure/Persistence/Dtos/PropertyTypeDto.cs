using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal class PropertyTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.PropertyType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string PropertyTypeGroupIdColumnName = "propertyTypeGroupId";
    public const string DataTypeIdColumnName = "dataTypeId";
    public const string ContentTypeIdColumnName = "contentTypeId";

    internal const string ReferenceColumnName = "DataTypeId"; // should be DataTypeIdColumnName, but for database compatibility we keep it like this
    internal const string ReferencePropertyTypeGroupIdColumnName = "PropertyTypeGroupId"; // should be PropertyTypeGroupIdColumnName, but for database compatibility we keep it like this

    private string? _alias;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = 100)]
    public int Id { get; set; }

    [Column(DataTypeIdColumnName)]
    [ForeignKey(typeof(DataTypeDto), Column = DataTypeDto.PrimaryKeyColumnName)]
    public int DataTypeId { get; set; }

    [Column(ContentTypeIdColumnName)]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int ContentTypeId { get; set; }

    [Column(PropertyTypeGroupIdColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(PropertyTypeGroupDto))]
    public int? PropertyTypeGroupId { get; set; }

    [Index(IndexTypes.NonClustered, Name = "IX_cmsPropertyTypeAlias")]
    [Column("Alias")]
    public string? Alias { get => _alias; set => _alias = value == null ? null : string.Intern(value); }

    [Column("Name")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }

    [Column("sortOrder")]
    [Constraint(Default = "0")]
    public int SortOrder { get; set; }

    [Column("mandatory")]
    [Constraint(Default = "0")]
    public bool Mandatory { get; set; }

    [Column("mandatoryMessage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(500)]
    public string? MandatoryMessage { get; set; }

    [Column("validationRegExp")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string? ValidationRegExp { get; set; }

    [Column("validationRegExpMessage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(500)]
    public string? ValidationRegExpMessage { get; set; }

    [Column("Description")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(2000)]
    public string? Description { get; set; }

    [Column("labelOnTop")]
    [Constraint(Default = "0")]
    public bool LabelOnTop { get; set; }

    [Column("variations")]
    [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
    public byte Variations { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = ReferenceColumnName)]
    public DataTypeDto DataTypeDto { get; set; } = null!;

    [Column("UniqueId")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyTypeUniqueID")]
    public Guid UniqueId { get; set; }
}

using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.PropertyType)]
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
    public string? Alias { get; set; }

    [Column("Name")]
    public string? Name { get; set; }

    [Column("PropertyTypeSortOrder")]
    public int SortOrder { get; set; }

    [Column("mandatory")]
    public bool Mandatory { get; set; }

    [Column("mandatoryMessage")]
    public string? MandatoryMessage { get; set; }

    [Column("validationRegExp")]
    public string? ValidationRegExp { get; set; }

    [Column("validationRegExpMessage")]
    public string? ValidationRegExpMessage { get; set; }

    [Column("Description")]
    public string? Description { get; set; }

    [Column("labelOnTop")]
    public bool LabelOnTop { get; set; }

    /* cmsMemberType */
    [Column("memberCanEdit")]
    public bool CanEdit { get; set; }

    [Column("viewOnProfile")]
    public bool ViewOnProfile { get; set; }

    [Column("isSensitive")]
    public bool IsSensitive { get; set; }

    /* DataType */
    [Column("propertyEditorAlias")]
    public string? PropertyEditorAlias { get; set; }

    [Column("dbType")]
    public string? DbType { get; set; }

    [Column("UniqueID")]
    public Guid UniqueId { get; set; }
}

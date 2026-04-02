using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey(PrimaryKeyColumnName)]
internal sealed class ExternalLoginTokenDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ExternalLoginToken;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string ExternalLoginIdColumnName = "externalLoginId";
    private const string NameColumnName = "name";

    /// <summary>
    /// Gets or sets the unique identifier for the external login token.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the associated external login record.
    /// </summary>
    [Column(ExternalLoginIdColumnName)]
    [ForeignKey(typeof(ExternalLoginDto), Column = ExternalLoginDto.PrimaryKeyColumnName)]
    public int ExternalLoginId { get; set; }

    /// <summary>
    /// Gets or sets the name of the external login token.
    /// </summary>
    [Column(NameColumnName)]
    [Length(255)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{ExternalLoginIdColumnName},{NameColumnName}", Name = "IX_" + TableName + "_Name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the external login token value.
    /// </summary>
    [Column("value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when the external login token was created.
    /// </summary>
    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the external login associated with this token.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(ExternalLoginId))]
    public ExternalLoginDto ExternalLoginDto { get; set; } = null!;
}

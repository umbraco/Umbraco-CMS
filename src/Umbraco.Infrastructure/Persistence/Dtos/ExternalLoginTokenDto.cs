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

    internal const string ReferenceMemberName = "ExternalLoginId"; // should be ExternalLoginIdColumnName, but for database compatibility we keep it like this

    private const string ExternalLoginIdColumnName = "externalLoginId";
    private const string NameColumnName = "name";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(ExternalLoginIdColumnName)]
    [ForeignKey(typeof(ExternalLoginDto), Column = ExternalLoginDto.PrimaryKeyColumnName)]
    public int ExternalLoginId { get; set; }

    [Column(NameColumnName)]
    [Length(255)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{ExternalLoginIdColumnName},{NameColumnName}", Name = "IX_" + TableName + "_Name")]
    public string Name { get; set; } = null!;

    [Column("value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Value { get; set; } = null!;

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = ReferenceMemberName)]
    public ExternalLoginDto ExternalLoginDto { get; set; } = null!;
}

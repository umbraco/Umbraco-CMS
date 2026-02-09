using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal class LongRunningOperationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.LongRunningOperation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoLongRunningOperation", AutoIncrement = false)]
    public Guid Id { get; set; }

    [Column("type")]
    [Length(50)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Type { get; set; } = null!;

    [Column("status")]
    [Length(50)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Status { get; set; } = null!;

    [Column("result")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Result { get; set; } = null;

    [Column("createDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    [Column("updateDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

    [Column("expirationDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime ExpirationDate { get; set; }
}

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class LockDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Lock;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoLock", AutoIncrement = false)]
    public int Id { get; set; }

    [Column("value")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public int Value { get; set; } = 1;

    [Column("name")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Length(64)]
    public string Name { get; set; } = null!;
}

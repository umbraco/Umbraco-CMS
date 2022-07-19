using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.Lock)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
internal class LockDto
{
    [Column("id")]
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

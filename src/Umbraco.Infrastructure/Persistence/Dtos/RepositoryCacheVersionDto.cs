using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("cacheIdentifier")]
[ExplicitColumns]
public class RepositoryCacheVersionDto
{
    internal const string TableName = Constants.DatabaseSchema.Tables.RepositoryCacheVersion;

    [Column("cacheIdentifier")]
    [Length(256)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true)]
    public required string CacheIdentifier { get; set; }

    [Column("version")]
    [Length(256)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Version { get; set; }
}

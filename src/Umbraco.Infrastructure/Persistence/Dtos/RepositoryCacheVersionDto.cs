using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("identifier", AutoIncrement = false)]
[ExplicitColumns]
public class RepositoryCacheVersionDto
{
    internal const string TableName = Constants.DatabaseSchema.Tables.RepositoryCacheVersion;

    [Column("identifier")]
    [Length(256)]
    [PrimaryKeyColumn(Name = "PK_umbracoRepositoryCacheVersion", AutoIncrement = false, Clustered = true)]
    public required string Identifier { get; set; }

    [Column("version")]
    [Length(256)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Version { get; set; }
}

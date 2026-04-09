using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object used for tracking repository cache version information
/// within the Umbraco CMS persistence layer. Typically contains versioning data to manage
/// cache consistency for repository operations.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class RepositoryCacheVersionDto
{
    internal const string TableName = Constants.DatabaseSchema.Tables.RepositoryCacheVersion;
    public const string PrimaryKeyColumnName = "identifier";

    /// <summary>
    /// Gets or sets the unique identifier of the repository cache version.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [Length(256)]
    [PrimaryKeyColumn(Name = "PK_umbracoRepositoryCacheVersion", AutoIncrement = false, Clustered = true)]
    public required string Identifier { get; set; }

    /// <summary>
    /// Gets or sets the string value representing the version of the repository cache.
    /// This value may be null.
    /// </summary>
    [Column("version")]
    [Length(256)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Version { get; set; }
}

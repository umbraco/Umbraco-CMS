using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) that encapsulates user consent details.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class ConsentDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Consent;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the consent record.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this consent is the current one.
    /// </summary>
    [Column("current")]
    public bool Current { get; set; }

    /// <summary>
    /// Gets or sets the origin or context from which the consent was obtained.
    /// </summary>
    [Column("source")]
    [Length(512)]
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the context in which the consent was given, such as the application or feature.
    /// </summary>
    [Column("context")]
    [Length(128)]
    public string? Context { get; set; }

    /// <summary>
    /// Gets or sets the name or description of the action associated with the consent. This value is mapped to the 'action' column in the database.
    /// </summary>
    [Column("action")]
    [Length(512)]
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the consent was created.
    /// </summary>
    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the state of the consent, represented as an integer value.
    /// The meaning of the state values depends on the consent implementation.
    /// </summary>
    [Column("state")]
    public int State { get; set; }

    /// <summary>
    /// Gets or sets the comment associated with the consent.
    /// </summary>
    [Column("comment")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Comment { get; set; }
}

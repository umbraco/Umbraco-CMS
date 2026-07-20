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

    /// <summary>
    /// Gets or sets the unique identifier for the long running operation.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoLongRunningOperation", AutoIncrement = false)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name or identifier representing the type of the long running operation.
    /// </summary>
    [Column("type")]
    [Length(200)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets the status of the long running operation, represented as a string (e.g., "Pending", "Running", "Completed").
    /// </summary>
    [Column("status")]
    [Length(50)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Status { get; set; } = null!;

    /// <summary>
    /// Gets or sets the string result of the long running operation, or <c>null</c> if no result is available.
    /// </summary>
    [Column("result")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Result { get; set; } = null;

    /// <summary>
    /// Gets or sets the date and time when the long running operation was created.
    /// </summary>
    [Column("createDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the operation was last updated.
    /// </summary>
    [Column("updateDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the long running operation expires.
    /// </summary>
    [Column("expirationDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime ExpirationDate { get; set; }
}

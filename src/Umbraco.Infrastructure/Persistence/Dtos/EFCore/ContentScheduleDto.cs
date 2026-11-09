using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ContentScheduleDtoConfiguration))]
public class ContentScheduleDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentSchedule;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string NodeIdColumnName = "nodeId";
    public const string LanguageIdColumnName = "languageId";
    public const string DateColumnName = "date";
    public const string ActionColumnName = "action";
    public const string IncludeInvariantForVariantColumnName = "includeInvariantForVariant";

    /// <summary>
    /// Gets or sets the unique identifier of the content schedule.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content node associated with this schedule.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the language associated with the content schedule.
    /// A null value indicates the schedule is invariant (not language-specific).
    /// </summary>
    public int? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the scheduled date and time for the content action.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the action to be performed as part of the content schedule.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether a scheduled release of variant content includes its invariant properties.
    /// </summary>
    public bool IncludeInvariantForVariant { get; set; }
}

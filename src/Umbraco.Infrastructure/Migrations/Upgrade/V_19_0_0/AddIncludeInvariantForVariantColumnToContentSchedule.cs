using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Adds the <c>includeInvariantForVariant</c> column to the <c>umbracoContentSchedule</c> table.
/// </summary>
/// <remarks>
///     Existing schedules are left without a value, so their scheduled releases include invariant properties as before.
/// </remarks>
public class AddIncludeInvariantForVariantColumnToContentSchedule : AsyncMigrationBase
{
    private const string ColumnName = "includeInvariantForVariant";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddIncludeInvariantForVariantColumnToContentSchedule"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddIncludeInvariantForVariantColumnToContentSchedule(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.ContentSchedule, ColumnName))
        {
            return Task.CompletedTask;
        }

        AddColumn<ContentScheduleDto>(Constants.DatabaseSchema.Tables.ContentSchedule, ColumnName);
        return Task.CompletedTask;
    }
}

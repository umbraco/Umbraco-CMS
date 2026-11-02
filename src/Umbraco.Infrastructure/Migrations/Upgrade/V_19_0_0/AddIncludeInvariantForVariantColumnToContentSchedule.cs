using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Adds the <c>includeInvariantForVariant</c> column to the <c>umbracoContentSchedule</c> table.
/// </summary>
/// <remarks>
///     Existing schedules are updated to include invariant properties, so their scheduled releases behave as before.
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

        Database.Execute(Sql().Update<ContentScheduleDto>(x => x.Set(c => c.IncludeInvariantForVariant, true)));

        return Task.CompletedTask;
    }
}

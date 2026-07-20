using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Represents a migration that converts all scheduled publish dates in the database to use UTC time.
/// </summary>
public class MigrateSchduledPublishesToUtc : MigrationBase
{
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateSchduledPublishesToUtc"/> class, which handles the migration of scheduled publish dates to UTC format during the upgrade to version 14.0.0.
    /// </summary>
    /// <param name="context">The migration context that provides information and services for the migration process.</param>
    /// <param name="timeProvider">An abstraction for obtaining the current time, used to ensure correct UTC conversion.</param>
    public MigrateSchduledPublishesToUtc(IMigrationContext context, TimeProvider timeProvider)
        : base(context)
    {
        _timeProvider = timeProvider;
    }

    protected override void Migrate()
    {
        var offset = _timeProvider.LocalTimeZone.BaseUtcOffset;
        var nowServerTime = _timeProvider.GetLocalNow();
        if (offset != TimeSpan.Zero)
        {
            var contentScheduleDtos = Database.Fetch<ContentScheduleDto>(Sql()
                .Select<ContentScheduleDto>()
                .From<ContentScheduleDto>()
                .Where<ContentScheduleDto>(x => x.Date > nowServerTime));

            foreach (ContentScheduleDto contentScheduleDto in contentScheduleDtos)
            {
                contentScheduleDto.Date = contentScheduleDto.Date.Subtract(offset);

                Database.Update(contentScheduleDto, x => x.Date);
            }
        }


    }
}

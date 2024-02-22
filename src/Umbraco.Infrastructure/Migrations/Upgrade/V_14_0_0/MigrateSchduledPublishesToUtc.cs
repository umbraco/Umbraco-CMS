using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class MigrateSchduledPublishesToUtc : MigrationBase
{
    private readonly TimeProvider _timeProvider;

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

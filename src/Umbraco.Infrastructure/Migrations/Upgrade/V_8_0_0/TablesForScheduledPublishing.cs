using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class TablesForScheduledPublishing : MigrationBase
{
    public TablesForScheduledPublishing(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // Get anything currently scheduled
        Sql? releaseSql = new Sql()
            .Select("nodeId", "releaseDate")
            .From("umbracoDocument")
            .Where("releaseDate IS NOT NULL");
        Dictionary<int, DateTime>? releases = Database.Dictionary<int, DateTime>(releaseSql);

        Sql? expireSql = new Sql()
            .Select("nodeId", "expireDate")
            .From("umbracoDocument")
            .Where("expireDate IS NOT NULL");
        Dictionary<int, DateTime>? expires = Database.Dictionary<int, DateTime>(expireSql);

        // drop old cols
        Delete.Column("releaseDate").FromTable("umbracoDocument").Do();
        Delete.Column("expireDate").FromTable("umbracoDocument").Do();

        // add new table
        Create.Table<ContentScheduleDto>(true).Do();

        // migrate the schedule
        foreach (KeyValuePair<int, DateTime> s in releases)
        {
            DateTime date = s.Value;
            var action = ContentScheduleAction.Release.ToString();

            Insert.IntoTable(ContentScheduleDto.TableName)
                .Row(new { id = Guid.NewGuid(), nodeId = s.Key, date, action })
                .Do();
        }

        foreach (KeyValuePair<int, DateTime> s in expires)
        {
            DateTime date = s.Value;
            var action = ContentScheduleAction.Expire.ToString();

            Insert.IntoTable(ContentScheduleDto.TableName)
                .Row(new { id = Guid.NewGuid(), nodeId = s.Key, date, action })
                .Do();
        }
    }
}

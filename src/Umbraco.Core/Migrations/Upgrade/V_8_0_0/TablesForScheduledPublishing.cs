using NPoco;
using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class TablesForScheduledPublishing : MigrationBase
    {
        public TablesForScheduledPublishing(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            //Get anything currently scheduled
            var scheduleSql = new Sql()
                .Select("nodeId", "releaseDate", "expireDate")
                .From("umbracoDocument")
                .Where("releaseDate IS NOT NULL OR expireDate IS NOT NULL");
            var schedules = Database.Dictionary<int, (DateTime? releaseDate, DateTime? expireDate)> (scheduleSql);

            //drop old cols
            Delete.Column("releaseDate").FromTable("umbracoDocument").Do();
            Delete.Column("expireDate").FromTable("umbracoDocument").Do();
            //add new table
            Create.Table<ContentScheduleDto>().Do();

            //migrate the schedule
            foreach(var s in schedules)
            {
                var date = s.Value.releaseDate;
                var action = ContentScheduleAction.Release.ToString();
                if (!date.HasValue)
                {
                    date = s.Value.expireDate;
                    action = ContentScheduleAction.Expire.ToString();
                }

                Insert.IntoTable(ContentScheduleDto.TableName)
                    .Row(new { nodeId = s.Key, date = date.Value, action = action })
                    .Do();
            }
        }
    }
}

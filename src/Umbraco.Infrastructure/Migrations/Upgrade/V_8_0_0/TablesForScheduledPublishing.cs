using System;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class TablesForScheduledPublishing : MigrationBase
    {
        public TablesForScheduledPublishing(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            //Get anything currently scheduled
            var releaseSql = new Sql()
                .Select("nodeId", "releaseDate")
                .From("umbracoDocument")
                .Where("releaseDate IS NOT NULL");
            var releases = Database.Dictionary<int, DateTime> (releaseSql);

            var expireSql = new Sql()
                .Select("nodeId", "expireDate")
                .From("umbracoDocument")
                .Where("expireDate IS NOT NULL");
            var expires = Database.Dictionary<int, DateTime>(expireSql);


            //drop old cols
            Delete.Column("releaseDate").FromTable("umbracoDocument").Do();
            Delete.Column("expireDate").FromTable("umbracoDocument").Do();
            //add new table
            Create.Table<ContentScheduleDto>(true).Do();

            //migrate the schedule
            foreach(var s in releases)
            {
                var date = s.Value;
                var action = ContentScheduleAction.Release.ToString();

                Insert.IntoTable(ContentScheduleDto.TableName)
                    .Row(new { id = Guid.NewGuid(), nodeId = s.Key, date = date, action = action })
                    .Do();
            }

            foreach (var s in expires)
            {
                var date = s.Value;
                var action = ContentScheduleAction.Expire.ToString();

                Insert.IntoTable(ContentScheduleDto.TableName)
                    .Row(new { id = Guid.NewGuid(), nodeId = s.Key, date = date, action = action })
                    .Do();
            }
        }
    }
}

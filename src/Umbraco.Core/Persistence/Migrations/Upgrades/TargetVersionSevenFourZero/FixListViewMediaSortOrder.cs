using System.Linq;
using NPoco;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourZero
{
    [Migration("7.4.0", 4, GlobalSettings.UmbracoMigrationName)]
    public class FixListViewMediaSortOrder : MigrationBase
    {
        public FixListViewMediaSortOrder(IMigrationContext context)
            : base(context)
        {
        }

        public override void Up()
        {
            var mediaListviewIncludeProperties = Context.Database.Fetch<DataTypePreValueDto>(
                Sql().SelectAll().From<DataTypePreValueDto>().Where<DataTypePreValueDto>(x => x.Id == -9)).FirstOrDefault();
            if (mediaListviewIncludeProperties != null)
            {
                if (mediaListviewIncludeProperties.Value.Contains("\"alias\":\"sort\""))
                {
                    mediaListviewIncludeProperties.Value = mediaListviewIncludeProperties.Value.Replace("\"alias\":\"sort\"", "\"alias\":\"sortOrder\"");
                    Context.Database.InsertOrUpdate(mediaListviewIncludeProperties);
                }
            }
        }

        public override void Down()
        {
        }
    }
}
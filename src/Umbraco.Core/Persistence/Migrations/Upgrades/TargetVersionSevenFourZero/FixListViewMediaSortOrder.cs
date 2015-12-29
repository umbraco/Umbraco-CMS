using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourZero
{
    [Migration("7.4.0", 4, GlobalSettings.UmbracoMigrationName)]
    public class FixListViewMediaSortOrder : MigrationBase
    {
        public FixListViewMediaSortOrder(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var mediaListviewIncludeProperties = Context.Database.Fetch<DataTypePreValueDto>(new Sql().Select("*").From<DataTypePreValueDto>(SqlSyntax).Where<DataTypePreValueDto>(x => x.Id == -9)).FirstOrDefault();
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
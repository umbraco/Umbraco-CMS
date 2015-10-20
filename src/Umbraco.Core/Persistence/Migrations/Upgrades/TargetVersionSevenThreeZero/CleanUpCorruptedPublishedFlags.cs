using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    /// <summary>
    /// Older corrupted dbs might have multiple published flags for a content item, this shouldn't be possible
    /// so we need to clear the content flag on the older version
    /// </summary>
    [Migration("7.3.0", 18, GlobalSettings.UmbracoMigrationName)]
    public class CleanUpCorruptedPublishedFlags : MigrationBase
    {
        public CleanUpCorruptedPublishedFlags(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }
        
        public override void Up()
        {
            //Get all cmsDocument items that have more than one published date set
            var sql = @"SELECT * FROM cmsDocument WHERE nodeId IN 
(SELECT nodeId
FROM cmsDocument
GROUP BY nodeId
HAVING SUM(CASE WHEN published = 0 THEN 0 ELSE 1 END) > 1)
AND published = 1
ORDER BY nodeId, updateDate";

            var docs = Context.Database.Fetch<DocumentDto>(sql).GroupBy(x => x.NodeId);

            foreach (var doc in docs)
            {
                var latest = doc.OrderByDescending(x => x.UpdateDate).First();
                foreach (var old in doc.Where(x => x.VersionId != latest.VersionId))
                {
                    Update.Table("cmsDocument").Set(new {published = 0}).Where(new {nodeId = old.NodeId});
                }
            }
        }

        public override void Down()
        {
        }
    }
}
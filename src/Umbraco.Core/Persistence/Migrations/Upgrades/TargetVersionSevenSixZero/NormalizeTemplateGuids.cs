using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class NormalizeTemplateGuids : MigrationBase
    {
        public NormalizeTemplateGuids(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            Execute.Code(UpdateTemplateGuids);
        }

        private static string UpdateTemplateGuids(Database database)
        {
            // we need this migration because ppl running pre-7.6 on Cloud and Courier have templates in different
            // environments having different GUIDs (Courier does not sync template GUIDs) and we need to normalize
            // these GUIDs so templates with the same alias on different environments have the same GUID.
            // however, if already running a prerelease version of 7.6, we do NOT want to normalize the GUIDs as quite
            // probably, we are already running Deploy and the GUIDs are OK. assuming noone is running a prerelease
            // of 7.6 on Courier.
            // so... testing if we already have a 7.6.0 version installed. not pretty but...?
            //
            var version = database.FirstOrDefault<string>("SELECT version FROM umbracoMigration WHERE name=@name ORDER BY version DESC", new { name = Constants.System.UmbracoMigrationName });
            if (version != null && version.StartsWith("7.6.0")) return string.Empty;

            var updates = database.Query<dynamic>(@"SELECT umbracoNode.id, cmsTemplate.alias FROM umbracoNode 
JOIN cmsTemplate ON umbracoNode.id=cmsTemplate.nodeId
WHERE nodeObjectType = @guid", new { guid = Constants.ObjectTypes.TemplateTypeGuid})
                .Select(template => Tuple.Create((int) template.id, ("template____" + (string) template.alias).ToGuid()))
                .ToList();

            foreach (var update in updates)
                database.Execute("UPDATE umbracoNode set uniqueId=@guid WHERE id=@id", new { guid = update.Item2, id = update.Item1 });

            return string.Empty;
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}

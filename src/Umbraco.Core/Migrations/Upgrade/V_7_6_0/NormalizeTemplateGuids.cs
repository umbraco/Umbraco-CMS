using System;
using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    public class NormalizeTemplateGuids : MigrationBase
    {
        public NormalizeTemplateGuids(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var database = Database;

            // we need this migration because ppl running pre-7.6 on Cloud and Courier have templates in different
            // environments having different GUIDs (Courier does not sync template GUIDs) and we need to normalize
            // these GUIDs so templates with the same alias on different environments have the same GUID.
            // however, if already running a prerelease version of 7.6, we do NOT want to normalize the GUIDs as quite
            // probably, we are already running Deploy and the GUIDs are OK. assuming noone is running a prerelease
            // of 7.6 on Courier.
            // so... testing if we already have a 7.6.0 version installed. not pretty but...?
            //
            var version = database.FirstOrDefault<string>("SELECT version FROM umbracoMigration WHERE name=@name ORDER BY version DESC", new { name = Constants.System.UmbracoUpgradePlanName });
            if (version != null && version.StartsWith("7.6.0")) return;

            var updates = database.Query<dynamic>(@"SELECT umbracoNode.id, cmsTemplate.alias FROM umbracoNode
JOIN cmsTemplate ON umbracoNode.id=cmsTemplate.nodeId
WHERE nodeObjectType = @guid", new { guid = Constants.ObjectTypes.TemplateType })
                .Select(template => Tuple.Create((int) template.id, ("template____" + (string) template.alias).ToGuid()))
                .ToList();

            foreach (var update in updates)
                database.Execute("UPDATE umbracoNode set uniqueId=@guid WHERE id=@id", new { guid = update.Item2, id = update.Item1 });
        }
    }
}

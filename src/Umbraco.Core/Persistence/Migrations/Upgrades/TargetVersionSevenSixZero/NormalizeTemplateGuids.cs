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
            var updates = database.Query<dynamic>("SELECT id, text FROM umbracoNode WHERE nodeObjectType = @guid", new { guid = Constants.ObjectTypes.TemplateTypeGuid})
                .Select(template => Tuple.Create((int) template.id, ("template____" + (string) template.text).ToGuid()))
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

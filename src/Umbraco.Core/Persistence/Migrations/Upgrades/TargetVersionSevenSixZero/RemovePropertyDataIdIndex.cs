using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    /// <summary>
    /// See: http://issues.umbraco.org/issue/U4-9188
    /// </summary>
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class UpdateUniqueIndexOnCmsPropertyData : MigrationBase
    {
        public UpdateUniqueIndexOnCmsPropertyData(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {            
            //tuple = tablename, indexname, columnname, unique
            var indexes = SqlSyntax.GetDefinedIndexes(Context.Database).ToArray();
            var found = indexes.FirstOrDefault(
                x => x.Item1.InvariantEquals("cmsPropertyData")
                     && x.Item2.InvariantEquals("IX_cmsPropertyData"));

            if (found != null)
            {
                //drop the index
                Delete.Index("IX_cmsPropertyData").OnTable("cmsPropertyData");             
            }
        }

        public override void Down()
        {
        }
    }
}
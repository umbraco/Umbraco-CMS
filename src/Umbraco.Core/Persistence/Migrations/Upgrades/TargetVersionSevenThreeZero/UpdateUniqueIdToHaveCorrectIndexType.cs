using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 5, Constants.System.UmbracoMigrationName)]
    public class UpdateUniqueIdToHaveCorrectIndexType : MigrationBase
    {
        public UpdateUniqueIdToHaveCorrectIndexType(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var indexes = SqlSyntax.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition(x)).ToArray();

            // drop the index if it exists
            if (indexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoNodeUniqueID")))
                Delete.Index("IX_umbracoNodeUniqueID").OnTable("umbracoNode");

            // set uniqueID to be non-nullable
            // the index *must* be dropped else 'one or more objects access this column' exception
            Alter.Table("umbracoNode").AlterColumn("uniqueID").AsGuid().NotNullable();

            // create the index
            Create.Index("IX_umbracoNode_uniqueID").OnTable("umbracoNode").OnColumn("uniqueID").Unique();
        }

        public override void Down()
        { }
    }
}

using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    //see: http://issues.umbraco.org/issue/U4-6188, http://issues.umbraco.org/issue/U4-6187

    [Migration("7.3.0", 5, Constants.System.UmbracoMigrationName)]
    public class UpdateUniqueIdToHaveCorrectIndexType : MigrationBase
    {
        public UpdateUniqueIdToHaveCorrectIndexType(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            var indexes = SqlSyntax.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

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

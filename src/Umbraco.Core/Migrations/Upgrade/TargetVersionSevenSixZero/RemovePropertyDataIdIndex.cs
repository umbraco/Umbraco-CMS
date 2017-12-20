using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSixZero
{
    /// <summary>
    /// See: http://issues.umbraco.org/issue/U4-9188
    /// </summary>
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class UpdateUniqueIndexOnPropertyData : MigrationBase
    {
        public UpdateUniqueIndexOnPropertyData(IMigrationContext context)
            : base(context)
        { }

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
                Delete.Index("IX_cmsPropertyData").OnTable("cmsPropertyData").Do();
            }
        }

        public override void Down()
        {
        }
    }
}

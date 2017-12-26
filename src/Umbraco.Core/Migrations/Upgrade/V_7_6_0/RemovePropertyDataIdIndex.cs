using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    /// <summary>
    /// See: http://issues.umbraco.org/issue/U4-9188
    /// </summary>
    public class UpdateUniqueIndexOnPropertyData : MigrationBase
    {
        public UpdateUniqueIndexOnPropertyData(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
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
    }
}

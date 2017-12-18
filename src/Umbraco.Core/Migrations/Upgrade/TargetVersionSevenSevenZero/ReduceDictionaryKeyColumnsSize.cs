using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 5, Constants.System.UmbracoMigrationName)]
    public class ReduceDictionaryKeyColumnsSize : MigrationBase
    {
        public ReduceDictionaryKeyColumnsSize(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            //Now we need to check if we can actually do this because we won't be able to if there's data in there that is too long

            Execute.Code(context =>
            {
                var database = context.Database;
                var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(database);

                var colLen = SqlSyntax is MySqlSyntaxProvider
                    ? database.ExecuteScalar<int?>(string.Format("select max(LENGTH({0})) from cmsDictionary", SqlSyntax.GetQuotedColumnName("key")))
                    : database.ExecuteScalar<int?>(string.Format("select max(datalength({0})) from cmsDictionary", SqlSyntax.GetQuotedColumnName("key")));

                if (colLen < 900 == false) return null;

                var local = Context.GetLocalMigration();

                //if it exists we need to drop it first
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsDictionary_key")))
                {
                    local.Delete.Index("IX_cmsDictionary_key").OnTable("cmsDictionary");
                }

                //we can apply the col length change
                local.Alter.Table("cmsDictionary")
                    .AlterColumn("key")
                    .AsString(450)
                    .NotNullable();

                return local.GetSql();
            });
        }
    }
}

using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 5, Constants.System.UmbracoMigrationName)]
    public class AddIndexToDictionaryKeyColumn : MigrationBase
    {
        public AddIndexToDictionaryKeyColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            Execute.Code(context =>
            {
                var database = context.Database;
                //Now we need to check if we can actually do this because we won't be able to if there's data in there that is too long
                var colLen = (SqlSyntax is MySqlSyntaxProvider)
                    ? database.ExecuteScalar<int?>(string.Format("select max(LENGTH({0})) from cmsDictionary", SqlSyntax.GetQuotedColumnName("key")))
                    : database.ExecuteScalar<int?>(string.Format("select max(datalength({0})) from cmsDictionary", SqlSyntax.GetQuotedColumnName("key")));

                if (colLen < 900 == false && colLen != null)
                {
                    return null;
                }

                var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(Context.Database);

                //make sure it doesn't already exist
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsDictionary_key")) == false)
                {
                    var local = Context.GetLocalMigration();

                    //we can apply the index
                    local.Create.Index("IX_cmsDictionary_key").OnTable("cmsDictionary")
                        .OnColumn("key")
                        .Ascending()
                        .WithOptions()
                        .NonClustered();

                    return local.GetSql();
                }

                return null;
            });
        }

        public override void Down()
        {
            Delete.Index("IX_cmsDictionary_key").OnTable("cmsDictionary");
        }
    }
}

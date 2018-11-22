using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 4, Constants.System.UmbracoMigrationName)]
    public class ReduceDictionaryKeyColumnsSize : MigrationBase
    {
        public ReduceDictionaryKeyColumnsSize(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            //Now we need to check if we can actually do this because we won't be able to if there's data in there that is too long

            Execute.Code(database =>
            {
                var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(database);

                var colLen = (SqlSyntax is MySqlSyntaxProvider)
                    ? database.ExecuteScalar<int?>(string.Format("select max(LENGTH({0})) from cmsDictionary", SqlSyntax.GetQuotedColumnName("key")))
                    : database.ExecuteScalar<int?>(string.Format("select max(datalength({0})) from cmsDictionary", SqlSyntax.GetQuotedColumnName("key")));

                if (colLen < 900 == false) return null;

                var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

                //if it exists we need to drop it first
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsDictionary_key")))
                {
                    localContext.Delete.Index("IX_cmsDictionary_key").OnTable("cmsDictionary");
                }

                //we can apply the col length change
                localContext.Alter.Table("cmsDictionary")
                    .AlterColumn("key")
                    .AsString(450)
                    .NotNullable();

                return localContext.GetSql();
            });
        }

        public override void Down()
        {

        }
    }
}
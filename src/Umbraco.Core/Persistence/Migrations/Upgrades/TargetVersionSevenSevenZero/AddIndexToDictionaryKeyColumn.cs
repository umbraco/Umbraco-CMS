using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 5, Constants.System.UmbracoMigrationName)]
    public class AddIndexToDictionaryKeyColumn : MigrationBase
    {
        public AddIndexToDictionaryKeyColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            Execute.Code(database =>
            {
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
                    var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

                    //we can apply the index
                    localContext.Create.Index("IX_cmsDictionary_key").OnTable("cmsDictionary")
                        .OnColumn("key")
                        .Ascending()
                        .WithOptions()
                        .NonClustered();

                    return localContext.GetSql();
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

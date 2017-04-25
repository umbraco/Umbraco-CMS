using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 3, Constants.System.UmbracoMigrationName)]
    public class AddIndexToCmsMemberLoginName : MigrationBase
    {
        public AddIndexToCmsMemberLoginName(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {   
            Execute.Code(database =>
            {
                //Now we need to check if we can actually d6 this because we won't be able to if there's data in there that is too long
                //http://issues.umbraco.org/issue/U4-9758

                var colLen = (SqlSyntax is MySqlSyntaxProvider)
                    ? database.ExecuteScalar<int?>("select max(LENGTH(LoginName)) from cmsMember")
                    : database.ExecuteScalar<int?>("select max(datalength(LoginName)) from cmsMember");

                if (colLen < 900 == false && colLen != null)
                {
                    return null;
                }

                var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(Context.Database);

                //make sure it doesn't already exist
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsMember_LoginName")) == false)
                {
                    var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

                    //we can apply the index
                    localContext.Create.Index("IX_cmsMember_LoginName").OnTable("cmsMember")
                        .OnColumn("LoginName")
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
            Delete.Index("IX_cmsMember_LoginName").OnTable("cmsMember");
        }
    }
}
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 2, Constants.System.UmbracoMigrationName)]
    public class ReduceLoginNameColumnsSize : MigrationBase
    {
        public ReduceLoginNameColumnsSize(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            //Now we need to check if we can actually d6 this because we won't be able to if there's data in there that is too long
            //http://issues.umbraco.org/issue/U4-9758

            Execute.Code(database =>
            {
                var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(database);

                var colLen = (SqlSyntax is MySqlSyntaxProvider)
                    ? database.ExecuteScalar<int?>("select max(LENGTH(LoginName)) from cmsMember")
                    : database.ExecuteScalar<int?>("select max(datalength(LoginName)) from cmsMember");

                if (colLen < 900 == false) return null;
                
                var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

                //if an index exists on this table we need to drop it. Normally we'd check via index name but in some odd cases (i.e. Our)
                //the index name is something odd (starts with "mi_"). In any case, the index cannot exist if we want to alter the column
                //so we'll drop whatever index is there and add one with the correct name after.
                var loginNameIndex = dbIndexes.FirstOrDefault(x => x.TableName.InvariantEquals("cmsMember") && x.ColumnName.InvariantEquals("LoginName"));
                if (loginNameIndex != null)
                {
                    localContext.Delete.Index(loginNameIndex.IndexName).OnTable("cmsMember");
                }

                //we can apply the col length change
                localContext.Alter.Table("cmsMember")
                    .AlterColumn("LoginName")
                    .AsString(225)
                    .NotNullable();

                return localContext.GetSql();
            });
        }

        public override void Down()
        {
            
        }
    }
}
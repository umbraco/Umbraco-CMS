using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    public class ReduceLoginNameColumnsSize : MigrationBase
    {
        public ReduceLoginNameColumnsSize(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            //Now we need to check if we can actually d6 this because we won't be able to if there's data in there that is too long
            //http://issues.umbraco.org/issue/U4-9758

            var database = Database;
            var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(database);

            var colLen = database.ExecuteScalar<int?>("select max(datalength(LoginName)) from cmsMember");

            if (colLen < 900 == false) return;

            //if an index exists on this table we need to drop it. Normally we'd check via index name but in some odd cases (i.e. Our)
            //the index name is something odd (starts with "mi_"). In any case, the index cannot exist if we want to alter the column
            //so we'll drop whatever index is there and add one with the correct name after.
            var loginNameIndex = dbIndexes.FirstOrDefault(x => x.TableName.InvariantEquals("cmsMember") && x.ColumnName.InvariantEquals("LoginName"));
            if (loginNameIndex != null)
            {
                Delete.Index(loginNameIndex.IndexName).OnTable("cmsMember").Do();
            }

            //we can apply the col length change
            Alter.Table("cmsMember")
                .AlterColumn("LoginName")
                .AsString(225)
                .NotNullable()
                .Do();
        }
    }
}

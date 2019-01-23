using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    public class AddIndexToCmsMemberLoginName : MigrationBase
    {
        public AddIndexToCmsMemberLoginName(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var database = Database;

            //Now we need to check if we can actually d6 this because we won't be able to if there's data in there that is too long
            //http://issues.umbraco.org/issue/U4-9758

            var colLen = database.ExecuteScalar<int?>("select max(datalength(LoginName)) from cmsMember");

            if (colLen < 900 == false && colLen != null)
            {
                return;
            }

            var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(Context.Database);

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsMember_LoginName")) == false)
            {
                //we can apply the index
                Create.Index("IX_cmsMember_LoginName").OnTable("cmsMember")
                    .OnColumn("LoginName")
                    .Ascending()
                    .WithOptions()
                    .NonClustered()
                    .Do();
            }
        }
    }
}

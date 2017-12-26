using System.Linq;
using System.Web.Security;
using Newtonsoft.Json;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Security;

namespace Umbraco.Core.Migrations.Upgrade.V_7_7_0
{
    public class UpdateUserTables : MigrationBase
    {
        public UpdateUserTables(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("createDate")) == false)
                Create.Column("createDate").OnTable("umbracoUser").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime).Do();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("updateDate")) == false)
                Create.Column("updateDate").OnTable("umbracoUser").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime).Do();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("emailConfirmedDate")) == false)
                Create.Column("emailConfirmedDate").OnTable("umbracoUser").AsDateTime().Nullable().Do();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("invitedDate")) == false)
                Create.Column("invitedDate").OnTable("umbracoUser").AsDateTime().Nullable().Do();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("avatar")) == false)
                Create.Column("avatar").OnTable("umbracoUser").AsString(500).Nullable().Do();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("passwordConfig")) == false)
            {
                Create.Column("passwordConfig").OnTable("umbracoUser").AsString(500).Nullable().Do();
                //Check if we have a known config, we only want to store config for hashing
                var membershipProvider = MembershipProviderExtensions.GetUsersMembershipProvider();
                if (membershipProvider.PasswordFormat == MembershipPasswordFormat.Hashed)
                {
                    var json = JsonConvert.SerializeObject(new { hashAlgorithm = Membership.HashAlgorithmType });
                    Database.Execute("UPDATE umbracoUser SET passwordConfig = '" + json + "'");
                }
            }
        }
    }
}

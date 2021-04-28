using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddPackagesSectionAccess : MigrationBase
    {
        public AddPackagesSectionAccess(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // Any user group which had access to the Developer section should have access to Packages
            Database.Execute($@"
                insert into {Constants.DatabaseSchema.Tables.UserGroup2App}
                select userGroupId, '{Constants.Applications.Packages}'
                from {Constants.DatabaseSchema.Tables.UserGroup2App}
                where app='developer'");
        }
    }
}

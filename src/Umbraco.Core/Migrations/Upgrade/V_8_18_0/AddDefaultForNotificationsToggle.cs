using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_18_0
{
    public class AddDefaultForNotificationsToggle : MigrationBase
    {
        public AddDefaultForNotificationsToggle(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            Update.Table(Constants.DatabaseSchema.Tables.UserGroup).Set("userGroupDefaultPermissions = userGroupDefaultPermissions + 'N'").Where("id = '1' OR id = '2' OR id = '3'").Do();
        }
    }
}

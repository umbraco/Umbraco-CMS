using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Migrations.Upgrade.V_7_12_0
{    public class UpdateUmbracoConsent : MigrationBase    {
        public UpdateUmbracoConsent(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()        {            Alter.Table(Constants.DatabaseSchema.Tables.Consent).AlterColumn("comment").AsString().Nullable().Do();        }
    }
}

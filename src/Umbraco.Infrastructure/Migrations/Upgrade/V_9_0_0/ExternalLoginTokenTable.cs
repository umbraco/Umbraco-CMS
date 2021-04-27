using System.Collections.Generic;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0
{

    public class ExternalLoginTokenTable : MigrationBase
    {
        public ExternalLoginTokenTable(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds new External Login token table
        /// </summary>
        public override void Migrate()
        {
            IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
            if (tables.InvariantContains(ExternalLoginTokenDto.TableName))
            {
                return;
            }

            Create.Table<ExternalLoginTokenDto>().Do();
        }
    }
}

using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenNineZero
{
    [Migration("7.9.0", 1, Constants.System.UmbracoMigrationName)]
    public class AddUmbracoConsentTable : MigrationBase
    {
        public AddUmbracoConsentTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains(ConsentDto.TableName))
                return;

            Create.Table<ConsentDto>();
        }

        public override void Down()
        {
        }
    }
}

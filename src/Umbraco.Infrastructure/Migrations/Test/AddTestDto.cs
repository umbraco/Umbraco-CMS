using System.Collections.Generic;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Test
{
    public class AddTestDto : MigrationBase
    {
        public AddTestDto(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds new columns to members table
        /// </summary>
        protected override void Migrate()
        {
            IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
            if (tables.InvariantContains(TestDto.TableName))
            {
                return;
            }

            Create.Table<TestDto>().Do();
        }
    }
}

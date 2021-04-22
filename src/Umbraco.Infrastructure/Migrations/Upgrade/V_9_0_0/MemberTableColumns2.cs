using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0
{
    public class MemberTableColumns2 : MigrationBase
    {
        public MemberTableColumns2(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds new columns to members table
        /// </summary>
        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<MemberDto>(columns, "passwordConfig");
        }
    }
}

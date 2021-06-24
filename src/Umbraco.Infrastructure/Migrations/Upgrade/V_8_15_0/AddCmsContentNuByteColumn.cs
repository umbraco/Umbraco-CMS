using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_15_0
{
    public class AddCmsContentNuByteColumn : MigrationBase
    {
        public AddCmsContentNuByteColumn(IMigrationContext context)
            : base(context)
        {

        }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<ContentNuDto>(columns, "dataRaw");
        }
    }
}

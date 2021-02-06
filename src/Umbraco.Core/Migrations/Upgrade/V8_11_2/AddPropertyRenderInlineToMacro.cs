using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V8_11_2
{
    public class AddPropertyRenderInlineToMacro : MigrationBase
    {
        public AddPropertyRenderInlineToMacro(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<MacroDto>(columns, "macroRenderInline");
        }
    }
}

using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    /// <summary>
    /// Adds a new, self-joined field to umbracoLanguages to hold the fall-back language for
    /// a given language.
    /// </summary>
    public class FallbackLanguage : MigrationBase
    {
        public FallbackLanguage(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.Language) && x.ColumnName.InvariantEquals("fallbackLanguageId")) == false)
                AddColumn<LanguageDto>("fallbackLanguageId");
        }
    }
}

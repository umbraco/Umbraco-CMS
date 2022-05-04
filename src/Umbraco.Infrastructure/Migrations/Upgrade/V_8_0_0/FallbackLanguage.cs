using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

/// <summary>
///     Adds a new, self-joined field to umbracoLanguages to hold the fall-back language for
///     a given language.
/// </summary>
public class FallbackLanguage : MigrationBase
{
    public FallbackLanguage(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        ColumnInfo[] columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

        if (columns.Any(x =>
                x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.Language) &&
                x.ColumnName.InvariantEquals("fallbackLanguageId")) == false)
        {
            AddColumn<LanguageDto>("fallbackLanguageId");
        }
    }
}

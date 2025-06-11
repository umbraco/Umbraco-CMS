using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_0_0;

public class AddSortOrderToLanguage : UnscopedMigrationBase
{
    private const string NewColumnName = "sortOrder";
    private readonly IScopeProvider _scopeProvider;

    public AddSortOrderToLanguage(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context)
        => _scopeProvider = scopeProvider;

    protected override void Migrate()
    {
        Logger.LogDebug("Adding sortOrder column to language.");

        if (TableExists(Constants.DatabaseSchema.Tables.Language))
        {
            var columns = Context.SqlContext.SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumn(columns, "sortOrder");
        }
        else
        {
            Logger.LogWarning($"Table {Constants.DatabaseSchema.Tables.Language} does not exist so the addition of the sortOrder column in migration {nameof(AddSortOrderToLanguage)} cannot be completed.");
        }
    }

    private void AddColumn(List<Persistence.SqlSyntax.ColumnInfo> columns, string column)
    {
        if (columns
            .SingleOrDefault(x => x.TableName == Constants.DatabaseSchema.Tables.Language && x.ColumnName == column) is null)
        {
            AddColumn<LanguageDto>(Constants.DatabaseSchema.Tables.Language, column);
        }
    }
}

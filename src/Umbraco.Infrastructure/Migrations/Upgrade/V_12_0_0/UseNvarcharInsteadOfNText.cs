using System.Linq.Expressions;
using System.Text;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

public class UseNvarcharInsteadOfNText : MigrationBase
{
    public UseNvarcharInsteadOfNText(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        MigrateNtextColumn<PropertyDataDto>("textValue", Constants.DatabaseSchema.Tables.PropertyData, x => x.TextValue);
    }

    private void MigrateNtextColumn<TDto>(string columnName, string tableName, Expression<Func<TDto, object?>> fieldSelector)
    {
        var oldColumnName = $"Old{columnName}";

        // Rename the column so we can create the new one and copy over the data.
        Rename
            .Column(columnName)
            .OnTable(tableName)
            .To(oldColumnName)
            .Do();

        // Create new column with the correct type
        // This should change between every column migration, so we'll request it each time.
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        AddColumnIfNotExists<TDto>(columns, columnName);

        // Copy over data NPOCO doesn't support this for some reason, so we'll have to do it like so
        // While we're add it we'll also set all the old values to be NULL since it's recommended here:
        // https://learn.microsoft.com/en-us/sql/t-sql/data-types/ntext-text-and-image-transact-sql?view=sql-server-ver16#remarks
        StringBuilder queryBuilder = new StringBuilder()
            .AppendLine($"UPDATE {tableName}")
            .AppendLine("SET")
            .AppendLine($"\t{SqlSyntax.GetFieldNameForUpdate(fieldSelector)} = {SqlSyntax.GetQuotedTableName(tableName)}.{SqlSyntax.GetQuotedColumnName(oldColumnName)},")
            .AppendLine($"\t{SqlSyntax.GetQuotedColumnName(oldColumnName)} = NULL");

        Sql<ISqlContext> copyDataQuery = Database.SqlContext.Sql(queryBuilder.ToString());
        Database.Execute(copyDataQuery);

        // Delete old column
        Delete
            .Column(oldColumnName)
            .FromTable(tableName)
            .Do();
    }
}

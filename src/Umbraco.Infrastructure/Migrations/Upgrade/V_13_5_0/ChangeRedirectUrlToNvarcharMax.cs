using System.Linq.Expressions;
using System.Text;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_5_0;

public class ChangeRedirectUrlToNvarcharMax : MigrationBase
{
    public ChangeRedirectUrlToNvarcharMax(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        // We don't need to run this migration for SQLite, since ntext is not a thing there, text is just text.
        if (DatabaseType == DatabaseType.SQLite)
        {
            return;
        }

        // There is an index with a dependency on the column, so we will have to remove it and recreate it after
        // because we are deleting the column it is based on
        Execute.Sql($"Drop Index IX_umbracoRedirectUrl_culture_hash on {Constants.DatabaseSchema.Tables.RedirectUrl}").Do();
        MigrateNtextColumn<RedirectUrlDto>("url", Constants.DatabaseSchema.Tables.RedirectUrl, x => x.Url, false);
        Execute.Sql($"CREATE INDEX IX_umbracoRedirectUrl_culture_hash ON {Constants.DatabaseSchema.Tables.RedirectUrl} (urlHash, contentKey, culture, createDateUtc)").Do();
    }

    private void MigrateNtextColumn<TDto>(string columnName, string tableName, Expression<Func<TDto, object?>> fieldSelector, bool nullable = true)
    {
        var columnType = ColumnType(tableName, columnName);
        if (columnType is null || columnType.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase) is false)
        {
            return;
        }

        var oldColumnName = $"Old{columnName}";

        // Rename the column so we can create the new one and copy over the data.
        Rename
            .Column(columnName)
            .OnTable(tableName)
            .To(oldColumnName)
            .Do();

        // Create new column with the correct type
        // This is pretty ugly, but we have to do it this way because the CacheInstruction.Instruction column doesn't support nullable.
        // So we have to populate with some temporary placeholder value before we copy over the actual data.
        ICreateColumnOptionBuilder builder = Create
            .Column(columnName)
            .OnTable(tableName)
            .AsCustom("nvarchar(max)");

        if (nullable is false)
        {
            builder
                .NotNullable()
                .WithDefaultValue("Placeholder");
        }
        else
        {
            builder.Nullable();
        }

        builder.Do();

        // Copy over data NPOCO doesn't support this for some reason, so we'll have to do it like so
        // While we're add it we'll also set all the old values to be NULL since it's recommended here:
        // https://learn.microsoft.com/en-us/sql/t-sql/data-types/ntext-text-and-image-transact-sql?view=sql-server-ver16#remarks
        StringBuilder queryBuilder = new StringBuilder()
            .AppendLine($"UPDATE {tableName}")
            .AppendLine("SET")
            .Append($"\t{SqlSyntax.GetFieldNameForUpdate(fieldSelector)} = {SqlSyntax.GetQuotedTableName(tableName)}.{SqlSyntax.GetQuotedColumnName(oldColumnName)}");

        if (nullable)
        {
            queryBuilder.AppendLine($"\n,\t{SqlSyntax.GetQuotedColumnName(oldColumnName)} = NULL");
        }

        Sql<ISqlContext> copyDataQuery = Database.SqlContext.Sql(queryBuilder.ToString());
        Database.Execute(copyDataQuery);

        // Delete old column
        Delete
            .Column(oldColumnName)
            .FromTable(tableName)
            .Do();
    }
}

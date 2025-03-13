using System.Linq.Expressions;
using System.Text;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

[Obsolete("This is no longer used and will be removed in V14.")]
public class UseNvarcharInsteadOfNText : MigrationBase
{
    public UseNvarcharInsteadOfNText(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // We don't need to run this migration for SQLite, since ntext is not a thing there, text is just text.
        if (DatabaseType == DatabaseType.SQLite)
        {
            return;
        }

        MigrateNtextColumn<ContentNuDto>("data", Constants.DatabaseSchema.Tables.NodeData, x => x.Data);
        MigrateNtextColumn<CacheInstructionDto>("jsonInstruction", Constants.DatabaseSchema.Tables.CacheInstruction, x => x.Instructions, false);
        MigrateNtextColumn<DataTypeDto>("config", Constants.DatabaseSchema.Tables.DataType, x => x.Configuration);
        MigrateNtextColumn<ExternalLoginDto>("userData", Constants.DatabaseSchema.Tables.ExternalLogin, x => x.UserData);
        MigrateNtextColumn<UserDto>("tourData", Constants.DatabaseSchema.Tables.User, x => x.TourData);
        MigrateNtextColumn<PropertyDataDto>("textValue", Constants.DatabaseSchema.Tables.PropertyData, x => x.TextValue);
    }

    private void RawMigrateNtextColumn(string columnName, string tableName)
    {
        var updateTypeSql = @$"
ALTER TABLE {tableName}
ALTER COLUMN {columnName} nvarchar(max)";
        Sql<ISqlContext> copyDataQuery = Database.SqlContext.Sql(updateTypeSql);
        Database.Execute(copyDataQuery);
    }

    private void MigrateNtextColumn<TDto>(string columnName, string tableName, Expression<Func<TDto, object?>> fieldSelector, bool nullable = true)
    {
        var columnType = ColumnType(tableName, columnName);
        if (columnType is null || columnType.Equals("ntext", StringComparison.InvariantCultureIgnoreCase) is false)
        {
            return;
        }

        // since it's ntext to nvarchar(max) we should be able to just raw query this without having issues with fallback values on sql server
        if (Database.DatabaseType != DatabaseType.SQLite)
        {
            RawMigrateNtextColumn(columnName, tableName);
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
        // This is pretty ugly, but we have to do ti this way because the CacheInstruction.Instruction column doesn't support nullable.
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

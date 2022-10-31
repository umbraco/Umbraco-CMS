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
        // Rename old Column
        Rename
            .Column("textValue")
            .OnTable(Constants.DatabaseSchema.Tables.PropertyData)
            .To("oldTextValue")
            .Do();

        // Create new column with the correct type
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        AddColumnIfNotExists<PropertyDataDto>(columns, "textValue");

        // Copy over data NPOCO doesn't support this for some insane reason, so we'll have to do it like so
        // While we're add it we'll also set all the old values to be NULL since it's recommended here:
        // https://learn.microsoft.com/en-us/sql/t-sql/data-types/ntext-text-and-image-transact-sql?view=sql-server-ver16#remarks
        StringBuilder queryBuilder = new StringBuilder()
            .AppendLine($"UPDATE {Constants.DatabaseSchema.Tables.PropertyData}")
            .AppendLine("SET")
            .AppendLine($"\t{Database.SqlContext.SqlSyntax.GetFieldNameForUpdate<PropertyDataDto>(x => x.TextValue)} = {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.PropertyData)}.{SqlSyntax.GetQuotedColumnName("oldTextValue")},")
            .AppendLine($"\t{SqlSyntax.GetQuotedColumnName("oldTextValue")} = NULL");

        Sql<ISqlContext> copyDataQuery = Database.SqlContext.Sql(queryBuilder.ToString());
        Database.Execute(copyDataQuery);

        // Delete old column
        Delete
            .Column("oldTextValue")
            .FromTable(Constants.DatabaseSchema.Tables.PropertyData)
            .Do();
    }
}

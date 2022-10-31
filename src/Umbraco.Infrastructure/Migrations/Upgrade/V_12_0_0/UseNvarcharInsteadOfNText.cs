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
    public UseNvarcharInsteadOfNText(IMigrationContext context) : base(context)
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

        // Copy over data NPOCO doesn't support this for some insane reason, so we'll have to do it like so:
        StringBuilder queryBuilder = new StringBuilder()
            .AppendLine($"UPDATE {Constants.DatabaseSchema.Tables.PropertyData}")
            .AppendLine("SET")
            .AppendLine($"\t{Database.SqlContext.SqlSyntax.GetFieldNameForUpdate<PropertyDataDto>(x => x.TextValue)} = {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.PropertyData)}.{SqlSyntax.GetQuotedColumnName("oldTextValue")},")
            .AppendLine($"\t{SqlSyntax.GetQuotedColumnName("oldTextValue")} = NULL");

        Sql<ISqlContext> copyDataQuery = Database.SqlContext.Sql(queryBuilder.ToString());
        Database.Execute(copyDataQuery);

        // Set old column to null
        // var setToNullQuery = Database.SqlContext.Sql()
        //     .Update<TempPropertyDataDto>(u => u.Set(x => x.OldTextValue, null));
        // Database.Execute(setToNullQuery);

        // Delete old column
        Delete.Column("oldTextValue").FromTable(Constants.DatabaseSchema.Tables.PropertyData).Do();
    }

    private class TempPropertyDataDto : PropertyDataDto
    {
        [Column("oldTextValue")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string? OldTextValue { get; set; }
    }
}

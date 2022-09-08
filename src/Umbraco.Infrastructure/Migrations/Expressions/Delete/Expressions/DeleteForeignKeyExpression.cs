using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

public class DeleteForeignKeyExpression : MigrationExpressionBase
{
    public DeleteForeignKeyExpression(IMigrationContext context)
        : base(context) =>
        ForeignKey = new ForeignKeyDefinition();

    public ForeignKeyDefinition ForeignKey { get; set; }

    protected override string GetSql()
    {
        if (ForeignKey.ForeignTable == null)
        {
            throw new ArgumentNullException(
                "Table name not specified, ensure you have appended the OnTable extension. Format should be Delete.ForeignKey(KeyName).OnTable(TableName)");
        }

        if (string.IsNullOrEmpty(ForeignKey.Name))
        {
            ForeignKey.Name =
                $"FK_{ForeignKey.ForeignTable}_{ForeignKey.PrimaryTable}_{ForeignKey.PrimaryColumns.First()}";
        }

        return string.Format(
            SqlSyntax.DeleteConstraint,
            SqlSyntax.GetQuotedTableName(ForeignKey.ForeignTable),
            SqlSyntax.GetQuotedName(ForeignKey.Name));
    }
}

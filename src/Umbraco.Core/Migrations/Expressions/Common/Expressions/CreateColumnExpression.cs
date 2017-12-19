using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Common.Expressions
{
    public class CreateColumnExpression : MigrationExpressionBase
    {
        public CreateColumnExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        {
            Column = new ColumnDefinition { ModificationType = ModificationType.Create };
        }

        public string TableName { get; set; }
        public ColumnDefinition Column { get; set; }

        public override string ToString() // fixme kill
            => GetSql();

        protected override string GetSql()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            if (string.IsNullOrEmpty(Column.TableName))
                Column.TableName = TableName;

            return string.Format(SqlSyntax.AddColumn,
                SqlSyntax.GetQuotedTableName(Column.TableName),
                SqlSyntax.Format(Column));
        }
    }
}

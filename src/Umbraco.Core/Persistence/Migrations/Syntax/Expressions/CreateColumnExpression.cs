using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateColumnExpression : MigrationExpressionBase
    {

        public CreateColumnExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
        {
            Column = new ColumnDefinition { ModificationType = ModificationType.Create };
        }

        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public ColumnDefinition Column { get; set; }

        public override string ToString()
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
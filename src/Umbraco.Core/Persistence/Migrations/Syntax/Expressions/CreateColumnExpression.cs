using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateColumnExpression : MigrationExpressionBase
    {
        public CreateColumnExpression(ISqlSyntaxProvider sqlSyntax, DatabaseProviders currentDatabaseProvider, DatabaseProviders[] supportedDatabaseProviders = null)
            : base(sqlSyntax, currentDatabaseProvider, supportedDatabaseProviders)
        {
            Column = new ColumnDefinition { ModificationType = ModificationType.Create };
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual ColumnDefinition Column { get; set; }

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
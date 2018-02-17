using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public class CheckForeignKeyForToTableBuilder : CheckForeignKeyBuilder, ICheckForeignKeyForToTableSyntax
    {
        public CheckForeignKeyForToTableBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckForeignKeyExpression expression) : base(context, databaseProviders, sqlSyntax, expression)
        {
        }

        public ICheckOptionSyntax WithColumn(string columnName)
        {
            var columnNames = new string[] { columnName };

            return WithColumns(columnNames);
        }
        public ICheckOptionSyntax WithColumns(string[] columnNames)
        {
            Expression.PrimaryColumnNames = columnNames;

            return this;
        }
    }
}

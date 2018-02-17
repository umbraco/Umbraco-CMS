using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public class CheckForeignKeyForFromTableBuilder : CheckForeignKeyBuilder, ICheckForeignKeyForFromTableSyntax
    {
        public CheckForeignKeyForFromTableBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckForeignKeyExpression expression) : base(context, databaseProviders, sqlSyntax, expression)
        {
        }

        public ICheckForeignKeyForFromTableOptionSyntax WithColumn(string columnName)
        {
            var columnNames = new string[] { columnName };

            return WithColumns(columnNames);
        }
        public ICheckForeignKeyForFromTableOptionSyntax WithColumns(string[] columnNames)
        {
            Expression.ForeignColumnNames = columnNames;

            return this;
        }
    }
}

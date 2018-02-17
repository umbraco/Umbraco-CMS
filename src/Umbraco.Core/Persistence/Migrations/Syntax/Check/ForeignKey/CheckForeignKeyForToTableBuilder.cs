using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public class CheckForeignKeyForToTableBuilder : CheckForeignKeyBuilder, ICheckForeignKeyForToTableSyntax
    {
        public CheckForeignKeyForToTableBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckForeignKeyExpression expression) : base(context, databaseProviders, sqlSyntax, expression)
        {
        }

        public ICheckExistsSyntax WithColumn(string columnName)
        {
            Expression.PrimaryColumnNames.Add(columnName);

            return this;
        }
        public ICheckExistsSyntax WithColumns(string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                Expression.PrimaryColumnNames.Add(columnName);
            }

            return this;
        }
    }
}

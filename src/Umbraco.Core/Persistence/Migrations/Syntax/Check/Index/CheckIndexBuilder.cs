using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Index
{
    public class CheckIndexBuilder : ExpressionBuilderBase<CheckIndexExpression>, ICheckIndexSyntax, ICheckIndexForTableSyntax, ICheckIndexOptionSyntax
    {
        private IMigrationContext _context;
        private DatabaseProviders[] _databaseProviders;
        private ISqlSyntaxProvider _sqlSyntax;

        public CheckIndexBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckIndexExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public ICheckIndexOptionSyntax WithColumn(string columnName)
        {
            return ColumnOption(columnName);
        }

        public ICheckIndexOptionSyntax WithColumns(string[] columnNames)
        {
            return ColumnsOption(columnNames);
        }

        private ICheckIndexOptionSyntax ColumnOption(string columnName)
        {
            Expression.ColumnNames.Add(columnName);

            return this;
        }

        private ICheckIndexOptionSyntax ColumnsOption(string[] columnNames)
        {
            foreach(var columnName in columnNames)
            {
                Expression.ColumnNames.Add(columnName);
            }

            return this;
        }

        public bool Exists()
        {
            var indexes = _sqlSyntax.GetDefinedIndexes(_context.Database);
            var foundIndexes = indexes.Where(x => x.Item2.InvariantEquals(Expression.IndexName));

            if (string.IsNullOrWhiteSpace(Expression.TableName) == false)
            {
                foundIndexes = foundIndexes.Where(x => x.Item1.InvariantEquals(Expression.TableName));
            }


            if (Expression.Unique.HasValue)
            {
                foundIndexes = foundIndexes.Where(x => x.Item4.Equals(Expression.Unique.Value));
            }

            if (Expression.ColumnNames.Any())
            {
                return Expression.ColumnNames.All(x =>
                                                foundIndexes.Any(c => c.Item2.InvariantEquals(x)
                                             ));
            }

            return foundIndexes.Any();
        }

        public ICheckOptionSyntax NotUnique()
        {
            Expression.Unique = false;
            return this;
        }

        public ICheckIndexOptionSyntax OnColumn(string columnName)
        {
            return ColumnOption(columnName);
        }

        public ICheckIndexOptionSyntax OnColumns(string[] columnNames)
        {
            return ColumnsOption(columnNames);
        }

        public ICheckIndexForTableSyntax OnTable(string tableName)
        {
            Expression.TableName = tableName;

            return this;
        }

        public ICheckOptionSyntax Unique()
        {
            Expression.Unique = true;

            return this;
        }
    }
}

using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public class CheckForeignKeyBuilder : ExpressionBuilderBase<CheckForeignKeyExpression>, ICheckForeignKeySyntax, ICheckForeignKeyFromTableSyntax, ICheckForeignKeyToTableSyntax
    {
        private IMigrationContext _context;
        private DatabaseProviders[] _databaseProviders;
        private ISqlSyntaxProvider _sqlSyntax;

        public CheckForeignKeyBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckForeignKeyExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public bool Exists()
        {
            var foundForeignKeys = _sqlSyntax.GetForeignKeys(_context.Database);
            

            if (string.IsNullOrWhiteSpace(Expression.ForeignKeyName) == false)
            {
                foundForeignKeys = foundForeignKeys.Where(x => x.Item5.InvariantEquals(Expression.ForeignKeyName));
            }

            if (string.IsNullOrWhiteSpace(Expression.ForeignTableName) == false)
            {
                foundForeignKeys = foundForeignKeys.Where(x => x.Item1.InvariantEquals(Expression.ForeignTableName));
            }

            if (string.IsNullOrWhiteSpace(Expression.PrimaryTableName) == false)
            {
                foundForeignKeys = foundForeignKeys.Where(x => x.Item3.InvariantEquals(Expression.PrimaryTableName));
            }

            return foundForeignKeys.Any();
        }

        public ICheckForeignKeyFromTableSyntax FromTable(string tableName)
        {
            Expression.ForeignTableName = tableName;

            return this;
        }

        public ICheckForeignKeyToTableSyntax ToTable(string tableName)
        {
            Expression.PrimaryTableName = tableName;

            return this;
        }
    }
}

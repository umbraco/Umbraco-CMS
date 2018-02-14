using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public class CheckColumnConstraintBuilder : ExpressionBuilderBase<CheckConstraintExpression>, ICheckColumnConstraintOptionSyntax
    {
        private IMigrationContext _context;
        private DatabaseProviders[] _databaseProviders;
        private ISqlSyntaxProvider _sqlSyntax;

        public CheckColumnConstraintBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckConstraintExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public ICheckOptionSyntax AndTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }

        public bool Exists()
        {
            var constraints = _sqlSyntax.GetConstraintsPerColumn(_context.Database);

            if (string.IsNullOrWhiteSpace(Expression.TableName))
            {
                return constraints.Any(x => x.Item2.InvariantEquals(Expression.ColumnName)
                                         && x.Item3.InvariantEquals(Expression.ConstraintName));
            }

            return constraints.Any(x => x.Item1.InvariantEquals(Expression.TableName)
                                     && x.Item2.InvariantEquals(Expression.ColumnName)
                                     && x.Item3.InvariantEquals(Expression.ConstraintName));
        }
    }
}

using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Column
{
    public class CheckColumnBuilder : ExpressionBuilderBase<CheckColumnExpression>, ICheckColumnOptionSyntax
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;

        public CheckColumnBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, CheckColumnExpression expression) : base(expression)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
        }

        public bool Exists()
        {
            return _sqlSyntax.GetColumnsInSchema(_context.Database).Any(x => x.TableName.InvariantEquals(Expression.TableName)
                                                                          && x.ColumnName.InvariantEquals(Expression.ColumnName));
        }
    }
}

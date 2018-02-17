using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Column
{
    public class CheckColumnsBuilder : ExpressionBuilderBase<CheckColumnsExpression>, ICheckExistsSyntax
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;
        private readonly ISqlSyntaxProvider _sqlSyntax;

        public CheckColumnsBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckColumnsExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public bool Exists()
        {
            var columns = _sqlSyntax.GetColumnsInSchema(_context.Database);

            return Expression.ColumnNames.All(x =>
                                                columns.Any(c =>
                                                            x.InvariantEquals(c.ColumnName)
                                                         && Expression.TableName.InvariantEquals(c.TableName)
                                                    )
                                              );

            
        }
    }
}

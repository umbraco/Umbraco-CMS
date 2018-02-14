using System;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Table
{
    public class CheckTableBuilder : ExpressionBuilderBase<CheckTableExpression>, ICheckTableOptionSyntax
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public CheckTableBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckTableExpression expression)
            : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public ICheckColumnOptionSyntax Column(string columnName)
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            return _sqlSyntax.DoesTableExist(_context.Database, Expression.TableName);
        }
    }
}

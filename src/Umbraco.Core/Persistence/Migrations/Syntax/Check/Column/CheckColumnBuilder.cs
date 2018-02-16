using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Column
{
    public class CheckColumnBuilder : ExpressionBuilderBase<CheckColumnExpression>, ICheckColumnOnTableSyntax
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;

        public CheckColumnBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, CheckColumnExpression expression) : base(expression)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
        }

        private ColumnInfo GetColumnInfo()
        {
            return _sqlSyntax.GetColumnsInSchema(_context.Database).FirstOrDefault(x => x.TableName.InvariantEquals(Expression.TableName)
                                                                                && x.ColumnName.InvariantEquals(Expression.ColumnName));
        }

        public bool Exists()
        {
            var column = GetColumnInfo();

            return Exists(column);
        }

        private bool Exists(ColumnInfo columnInfo)
        {
            return GetColumnInfo() != default(ColumnInfo);
        }

        public bool IsNotNullable()
        {
            var column = GetColumnInfo();

            return Exists(column) && column.IsNullable == false;
        }

        public bool IsNullable()
        {
            var column = GetColumnInfo();

            return Exists(column) && column.IsNullable;
        }

        public bool IsDataType(string dataType)
        {
            var column = GetColumnInfo();

            return Exists() && column.DataType.InvariantEquals(dataType);
        }
    }
}

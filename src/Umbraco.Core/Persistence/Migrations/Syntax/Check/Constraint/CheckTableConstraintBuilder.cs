﻿using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public class CheckTableConstraintBuilder : ExpressionBuilderBase<CheckConstraintExpression>, ICheckTableConstraintOptionSyntax
    {
        private IMigrationContext _context;
        private DatabaseProviders[] _databaseProviders;
        private ISqlSyntaxProvider _sqlSyntax;

        public CheckTableConstraintBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckConstraintExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public ICheckOptionSyntax WithColumn(string columnName)
        {
            var columnNames = new string[] { columnName };

            return WithColumns(columnNames);
        }

        public ICheckOptionSyntax WithColumns(string[] columnNames)
        {
            var expression = new CheckConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax)
            {
                ConstraintName = Expression.ConstraintName,
                TableName = Expression.TableName,
                ColumnNames = columnNames
            };

            return new CheckColumnsConstraintBuilder(_context, _databaseProviders, _sqlSyntax, expression);
        }


        public bool Exists()
        {
            return _sqlSyntax.GetConstraintsPerTable(_context.Database).Any(x => x.Item1.InvariantEquals(Expression.TableName)
                                                                              && x.Item2.InvariantEquals(Expression.ConstraintName));
        }
    }
}

﻿using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public class CheckColumnsConstraintBuilder : ExpressionBuilderBase<CheckConstraintExpression>, ICheckColumnsConstraintOptionSyntax
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;
        private readonly ISqlSyntaxProvider _sqlSyntax;

        public CheckColumnsConstraintBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckConstraintExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public ICheckExistsSyntax AndTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }

        public bool Exists()
        {
            var constraints = _sqlSyntax.GetConstraintsPerColumn(_context.Database);

            if (string.IsNullOrWhiteSpace(Expression.TableName))
            {
                return Expression.ColumnNames.All(x =>
                                                    constraints.Any(c => c.Item2.InvariantEquals(x)
                                                                 && c.Item3.InvariantEquals(Expression.ConstraintName)
                                                  ));
            }

            return Expression.ColumnNames.All(x =>
                                                constraints.Any(c => c.Item1.InvariantEquals(Expression.TableName)
                                                                  && c.Item2.InvariantEquals(x)
                                                                  && c.Item3.InvariantEquals(Expression.ConstraintName)
                                             ));
        }
    }
}

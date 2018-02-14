﻿using Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Table;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check
{
    public class CheckBuilder : ICheckBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public CheckBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        public ICheckConstraintOptionSyntax Constraint(string constraintName)
        {
            var expression = new CheckConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax)
            {
                ConstraintName = constraintName
            };

            return new CheckConstraintBuilder(_context, _databaseProviders, _sqlSyntax, expression);
        }

        public ICheckTableOptionSyntax Table(string tableName)
        {
            var expression = new CheckTableExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax)
            {
                TableName = tableName
            };

            return new CheckTableBuilder(_context, _databaseProviders, _sqlSyntax, expression);
        }
    }
}

using System;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Index;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Table;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create
{
    public class CreateBuilder : ICreateBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public CreateBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (sqlSyntax == null) throw new ArgumentNullException("sqlSyntax");

            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        public ICreateTableWithColumnSyntax Table(string tableName)
        {
            var expression = new CreateTableExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new CreateTableBuilder(_context, _databaseProviders, expression);
        }

        public ICreateColumnOnTableSyntax Column(string columnName)
        {
            var expression = new CreateColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { Column = { Name = columnName } };
            _context.Expressions.Add(expression);
            return new CreateColumnBuilder(_context, _databaseProviders, expression);
        }

        public ICreateForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new CreateForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax);
            _context.Expressions.Add(expression);
            return new CreateForeignKeyBuilder(expression);
        }

        public ICreateForeignKeyFromTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new CreateForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { ForeignKey = { Name = foreignKeyName } };
            _context.Expressions.Add(expression);
            return new CreateForeignKeyBuilder(expression);
        }

        public ICreateIndexForTableSyntax Index()
        {
            var expression = new CreateIndexExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax);
            _context.Expressions.Add(expression);
            return new CreateIndexBuilder(expression);
        }

        public ICreateIndexForTableSyntax Index(string indexName)
        {
            var expression = new CreateIndexExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { Index = { Name = indexName } };
            _context.Expressions.Add(expression);
            return new CreateIndexBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax PrimaryKey()
        {
            var expression = new CreateConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax, ConstraintType.PrimaryKey);
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new CreateConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax, ConstraintType.PrimaryKey);
            expression.Constraint.ConstraintName = primaryKeyName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax UniqueConstraint()
        {
            var expression = new CreateConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax, ConstraintType.Unique);
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax, ConstraintType.Unique);
            expression.Constraint.ConstraintName = constraintName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax Constraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax, ConstraintType.NonUnique);
            expression.Constraint.ConstraintName = constraintName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }
    }
}
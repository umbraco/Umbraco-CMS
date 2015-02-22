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
        private readonly DatabaseProviders[] _databaseProviders;

        public CreateBuilder(IMigrationContext context, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _databaseProviders = databaseProviders;
        }

        public ICreateTableWithColumnSyntax Table(string tableName)
        {
            var expression = new CreateTableExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, _databaseProviders)
            {
                TableName = tableName
            };
            _context.Expressions.Add(expression);
            return new CreateTableBuilder(expression, _context);
        }

        public ICreateColumnOnTableSyntax Column(string columnName)
        {
            var expression = new CreateColumnExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, _databaseProviders)
            {
                Column = { Name = columnName }
            };
            _context.Expressions.Add(expression);
            return new CreateColumnBuilder(expression, _context);
        }

        public ICreateForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new CreateForeignKeyExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, _databaseProviders);
            _context.Expressions.Add(expression);
            return new CreateForeignKeyBuilder(expression);
        }

        public ICreateForeignKeyFromTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new CreateForeignKeyExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, _databaseProviders)
            {
                ForeignKey = { Name = foreignKeyName }
            };
            _context.Expressions.Add(expression);
            return new CreateForeignKeyBuilder(expression);
        }

        public ICreateIndexForTableSyntax Index()
        {
            var expression = new CreateIndexExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, _databaseProviders);
            _context.Expressions.Add(expression);
            return new CreateIndexBuilder(expression);
        }

        public ICreateIndexForTableSyntax Index(string indexName)
        {
            var expression = new CreateIndexExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, _databaseProviders)
            {
                Index = { Name = indexName }
            };
            _context.Expressions.Add(expression);
            return new CreateIndexBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax PrimaryKey()
        {
            var expression = new CreateConstraintExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, ConstraintType.PrimaryKey, _databaseProviders);
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new CreateConstraintExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, ConstraintType.PrimaryKey, _databaseProviders);
            expression.Constraint.ConstraintName = primaryKeyName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax UniqueConstraint()
        {
            var expression = new CreateConstraintExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, ConstraintType.Unique, _databaseProviders);
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, ConstraintType.Unique, _databaseProviders);
            expression.Constraint.ConstraintName = constraintName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax Constraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider, ConstraintType.NonUnique, _databaseProviders);
            expression.Constraint.ConstraintName = constraintName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }
    }
}
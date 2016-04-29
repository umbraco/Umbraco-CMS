using System;
using NPoco;
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
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public CreateBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));            
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public ICreateTableWithColumnSyntax Table(string tableName)
        {
            var expression = new CreateTableExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new CreateTableBuilder(_context, _supportedDatabaseTypes, expression);
        }

        public ICreateColumnOnTableSyntax Column(string columnName)
        {
            var expression = new CreateColumnExpression(_context, _supportedDatabaseTypes) { Column = { Name = columnName } };
            _context.Expressions.Add(expression);
            return new CreateColumnBuilder(_context, _supportedDatabaseTypes, expression);
        }

        public ICreateForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new CreateForeignKeyExpression(_context, _supportedDatabaseTypes);
            _context.Expressions.Add(expression);
            return new CreateForeignKeyBuilder(expression);
        }

        public ICreateForeignKeyFromTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new CreateForeignKeyExpression(_context, _supportedDatabaseTypes) { ForeignKey = { Name = foreignKeyName } };
            _context.Expressions.Add(expression);
            return new CreateForeignKeyBuilder(expression);
        }

        public ICreateIndexForTableSyntax Index()
        {
            var expression = new CreateIndexExpression(_context, _supportedDatabaseTypes);
            _context.Expressions.Add(expression);
            return new CreateIndexBuilder(expression);
        }

        public ICreateIndexForTableSyntax Index(string indexName)
        {
            var expression = new CreateIndexExpression(_context, _supportedDatabaseTypes) { Index = { Name = indexName } };
            _context.Expressions.Add(expression);
            return new CreateIndexBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax PrimaryKey()
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.PrimaryKey);
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.PrimaryKey);
            expression.Constraint.ConstraintName = primaryKeyName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax UniqueConstraint()
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.Unique);
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.Unique);
            expression.Constraint.ConstraintName = constraintName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }

        public ICreateConstraintOnTableSyntax Constraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.NonUnique);
            expression.Constraint.ConstraintName = constraintName;
            _context.Expressions.Add(expression);
            return new CreateConstraintBuilder(expression);
        }
    }
}
﻿using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.DefaultConstraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Index;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete
{
    public class DeleteBuilder : IDeleteBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public DeleteBuilder(IMigrationContext context, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _databaseProviders = databaseProviders;
        }

        public void Table(string tableName)
        {
            var expression = new DeleteTableExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax) { TableName = tableName };
            _context.Expressions.Add(expression);
        }

        public IDeleteColumnFromTableSyntax Column(string columnName)
        {
            var expression = new DeleteColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax) {ColumnNames = {columnName}};
            _context.Expressions.Add(expression);
            return new DeleteColumnBuilder(expression);
        }

        public IDeleteForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new DeleteForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax);
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyBuilder(expression);
        }

        public IDeleteForeignKeyOnTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new DeleteForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax) {ForeignKey = {Name = foreignKeyName}};
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyBuilder(expression);
        }

        public IDeleteDataSyntax FromTable(string tableName)
        {
            var expression = new DeleteDataExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new DeleteDataBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index()
        {
            var expression = new DeleteIndexExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax);
            _context.Expressions.Add(expression);
            return new DeleteIndexBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index(string indexName)
        {
            var expression = new DeleteIndexExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax) { Index = { Name = indexName } };
            _context.Expressions.Add(expression);
            return new DeleteIndexBuilder(expression);
        }

        public IDeleteConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new DeleteConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax, ConstraintType.PrimaryKey)
            {
                Constraint = { ConstraintName = primaryKeyName }
            };
            _context.Expressions.Add(expression);
            return new DeleteConstraintBuilder(expression);
        }

        public IDeleteConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new DeleteConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax, ConstraintType.Unique)
            {
                Constraint = { ConstraintName = constraintName }
            };
            _context.Expressions.Add(expression);
            return new DeleteConstraintBuilder(expression);
        }

        public IDeleteDefaultConstraintOnTableSyntax DefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _context.SqlSyntax);
            _context.Expressions.Add(expression);
            return new DeleteDefaultConstraintBuilder(expression);
        }
    }
}
using System;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.DefaultConstraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Index;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete
{
    public class DeleteBuilder : IDeleteBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public DeleteBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        [Obsolete("Use the other constructor specifying an ISqlSyntaxProvider instead")]
        public DeleteBuilder(IMigrationContext context, params DatabaseProviders[] databaseProviders)
            : this(context, SqlSyntaxContext.SqlSyntaxProvider, databaseProviders)
        {
        }

        public void Table(string tableName)
        {
            var expression = new DeleteTableExpression { TableName = tableName };
            _context.Expressions.Add(expression);
        }

        public IDeleteColumnFromTableSyntax Column(string columnName)
        {
            var expression = _databaseProviders == null
                ? new DeleteColumnExpression { ColumnNames = { columnName } }
                : new DeleteColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders) { ColumnNames = { columnName } };
            _context.Expressions.Add(expression);
            return new DeleteColumnBuilder(expression);
        }

        public IDeleteForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = _databaseProviders == null
                ? new DeleteForeignKeyExpression(_sqlSyntax)
                : new DeleteForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax);
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyBuilder(expression);
        }

        public IDeleteForeignKeyOnTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = _databaseProviders == null
                ? new DeleteForeignKeyExpression(_sqlSyntax) { ForeignKey = { Name = foreignKeyName } }
                : new DeleteForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { ForeignKey = { Name = foreignKeyName } };
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyBuilder(expression);
        }

        public IDeleteDataSyntax FromTable(string tableName)
        {
            var expression = new DeleteDataExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new DeleteDataBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index()
        {
            var expression = new DeleteIndexExpression();
            _context.Expressions.Add(expression);
            return new DeleteIndexBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index(string indexName)
        {
            var expression = new DeleteIndexExpression { Index = { Name = indexName } };
            _context.Expressions.Add(expression);
            return new DeleteIndexBuilder(expression);
        }

        public IDeleteConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new DeleteConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, ConstraintType.PrimaryKey)
            {
                Constraint = { ConstraintName = primaryKeyName }
            };
            _context.Expressions.Add(expression);
            return new DeleteConstraintBuilder(expression);
        }

        public IDeleteConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique)
            {
                Constraint = { ConstraintName = constraintName }
            };
            _context.Expressions.Add(expression);
            return new DeleteConstraintBuilder(expression);
        }

        public IDeleteDefaultConstraintOnTableSyntax DefaultConstraint()
        {
            var expression = _databaseProviders == null
                ? new DeleteDefaultConstraintExpression()
                : new DeleteDefaultConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders);
            _context.Expressions.Add(expression);
            return new DeleteDefaultConstraintBuilder(expression);
        }
    }
}
using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
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
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public DeleteBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public void Table(string tableName)
        {
            var expression = new DeleteTableExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            _context.Expressions.Add(expression);
        }

        public IDeleteColumnFromTableSyntax Column(string columnName)
        {
            var expression = new DeleteColumnExpression(_context, _supportedDatabaseTypes) {ColumnNames = {columnName}};
            _context.Expressions.Add(expression);
            return new DeleteColumnBuilder(expression);
        }

        public IDeleteForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new DeleteForeignKeyExpression(_context, _supportedDatabaseTypes);
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyBuilder(expression);
        }

        public IDeleteForeignKeyOnTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new DeleteForeignKeyExpression(_context, _supportedDatabaseTypes) {ForeignKey = {Name = foreignKeyName}};
            _context.Expressions.Add(expression);
            return new DeleteForeignKeyBuilder(expression);
        }

        public IDeleteDataSyntax FromTable(string tableName)
        {
            var expression = new DeleteDataExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new DeleteDataBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index()
        {
            var expression = new DeleteIndexExpression(_context, _supportedDatabaseTypes);
            _context.Expressions.Add(expression);
            return new DeleteIndexBuilder(expression);
        }

        public IDeleteIndexForTableSyntax Index(string indexName)
        {
            var expression = new DeleteIndexExpression(_context, _supportedDatabaseTypes) { Index = { Name = indexName } };
            _context.Expressions.Add(expression);
            return new DeleteIndexBuilder(expression);
        }

        public IDeleteConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new DeleteConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.PrimaryKey)
            {
                Constraint = { ConstraintName = primaryKeyName }
            };
            _context.Expressions.Add(expression);
            return new DeleteConstraintBuilder(expression);
        }

        public IDeleteConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new DeleteConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.Unique)
            {
                Constraint = { ConstraintName = constraintName }
            };
            _context.Expressions.Add(expression);
            return new DeleteConstraintBuilder(expression);
        }

        public IDeleteDefaultConstraintOnTableSyntax DefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression(_context, _supportedDatabaseTypes);
            _context.Expressions.Add(expression);
            return new DeleteDefaultConstraintBuilder(expression);
        }
    }
}
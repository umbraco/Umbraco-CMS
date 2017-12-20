using NPoco;
using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Delete.Column;
using Umbraco.Core.Migrations.Expressions.Delete.Constraint;
using Umbraco.Core.Migrations.Expressions.Delete.Data;
using Umbraco.Core.Migrations.Expressions.Delete.DefaultConstraint;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;
using Umbraco.Core.Migrations.Expressions.Delete.ForeignKey;
using Umbraco.Core.Migrations.Expressions.Delete.Index;
using Umbraco.Core.Migrations.Expressions.Delete.KeysAndIndexes;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Delete
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

        /// <inheritdoc />
        public IExecutableBuilder Table(string tableName)
        {
            var expression = new DeleteTableExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            return new ExecutableBuilder(expression);
        }

        /// <inheritdoc />
        public IExecutableBuilder KeysAndIndexes(string tableName = null)
        {
            return new DeleteKeysAndIndexesBuilder(_context, _supportedDatabaseTypes) { TableName = tableName };
        }

        /// <inheritdoc />
        public IDeleteColumnBuilder Column(string columnName)
        {
            var expression = new DeleteColumnExpression(_context, _supportedDatabaseTypes) {ColumnNames = {columnName}};
            return new DeleteColumnBuilder(expression);
        }

        /// <inheritdoc />
        public IDeleteForeignKeyFromTableBuilder ForeignKey()
        {
            var expression = new DeleteForeignKeyExpression(_context, _supportedDatabaseTypes);
            return new DeleteForeignKeyBuilder(expression);
        }

        /// <inheritdoc />
        public IDeleteForeignKeyOnTableBuilder ForeignKey(string foreignKeyName)
        {
            var expression = new DeleteForeignKeyExpression(_context, _supportedDatabaseTypes) {ForeignKey = {Name = foreignKeyName}};
            return new DeleteForeignKeyBuilder(expression);
        }

        /// <inheritdoc />
        public IDeleteDataBuilder FromTable(string tableName)
        {
            var expression = new DeleteDataExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            return new DeleteDataBuilder(expression);
        }

        /// <inheritdoc />
        public IDeleteIndexForTableBuilder Index()
        {
            var expression = new DeleteIndexExpression(_context, _supportedDatabaseTypes);
            return new DeleteIndexBuilder(expression);
        }

        /// <inheritdoc />
        public IDeleteIndexForTableBuilder Index(string indexName)
        {
            var expression = new DeleteIndexExpression(_context, _supportedDatabaseTypes) { Index = { Name = indexName } };
            return new DeleteIndexBuilder(expression);
        }

        /// <inheritdoc />
        public IDeleteConstraintBuilder PrimaryKey(string primaryKeyName)
        {
            var expression = new DeleteConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.PrimaryKey)
            {
                Constraint = { ConstraintName = primaryKeyName }
            };
            return new DeleteConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public IDeleteConstraintBuilder UniqueConstraint(string constraintName)
        {
            var expression = new DeleteConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.Unique)
            {
                Constraint = { ConstraintName = constraintName }
            };
            return new DeleteConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public IDeleteDefaultConstraintOnTableBuilder DefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression(_context, _supportedDatabaseTypes);
            return new DeleteDefaultConstraintBuilder(expression);
        }
    }
}

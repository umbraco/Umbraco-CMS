using NPoco;
using System.Linq;
using Umbraco.Core.Migrations.Expressions.Delete.Column;
using Umbraco.Core.Migrations.Expressions.Delete.Constraint;
using Umbraco.Core.Migrations.Expressions.Delete.Data;
using Umbraco.Core.Migrations.Expressions.Delete.DefaultConstraint;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;
using Umbraco.Core.Migrations.Expressions.Delete.ForeignKey;
using Umbraco.Core.Migrations.Expressions.Delete.Index;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Expressions.Delete
{
    /// <summary>
    /// Implements <see cref="IDeleteBuilder"/>.
    /// </summary>
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
        public void Table(string tableName)
        {
            var expression = new DeleteTableExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            expression.Execute();
        }

        /// <inheritdoc />
        public void KeysAndIndexes(string tableName = null)
        {
            if (tableName == null)
            {
                // drop keys
                var keys = _context.SqlContext.SqlSyntax.GetConstraintsPerTable(_context.Database).DistinctBy(x => x.Item2).ToArray();
                foreach (var key in keys.Where(x => x.Item2.StartsWith("FK_")))
                    ForeignKey(key.Item2).OnTable(key.Item1);
                foreach (var key in keys.Where(x => x.Item2.StartsWith("PK_")))
                    PrimaryKey(key.Item2).FromTable(key.Item1);

                // drop indexes
                var indexes = _context.SqlContext.SqlSyntax.GetDefinedIndexesDefinitions(_context.Database).DistinctBy(x => x.IndexName).ToArray();
                foreach (var index in indexes)
                    Index(index.IndexName).OnTable(index.TableName);
            }
            else
            {
                // drop keys
                var keys = _context.SqlContext.SqlSyntax.GetConstraintsPerTable(_context.Database).DistinctBy(x => x.Item2).ToArray();
                foreach (var key in keys.Where(x => x.Item1 == tableName && x.Item2.StartsWith("FK_")))
                    ForeignKey(key.Item2).OnTable(key.Item1);
                foreach (var key in keys.Where(x => x.Item1 == tableName && x.Item2.StartsWith("PK_")))
                    PrimaryKey(key.Item2).FromTable(key.Item1);

                // drop indexes
                var indexes = _context.SqlContext.SqlSyntax.GetDefinedIndexesDefinitions(_context.Database).DistinctBy(x => x.IndexName).ToArray();
                foreach (var index in indexes.Where(x => x.TableName == tableName))
                    Index(index.IndexName).OnTable(index.TableName);
            }
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

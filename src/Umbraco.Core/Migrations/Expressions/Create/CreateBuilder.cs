using System;
using NPoco;
using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Common.Expressions;
using Umbraco.Core.Migrations.Expressions.Create.Column;
using Umbraco.Core.Migrations.Expressions.Create.Constraint;
using Umbraco.Core.Migrations.Expressions.Create.Expressions;
using Umbraco.Core.Migrations.Expressions.Create.ForeignKey;
using Umbraco.Core.Migrations.Expressions.Create.Index;
using Umbraco.Core.Migrations.Expressions.Create.KeysAndIndexes;
using Umbraco.Core.Migrations.Expressions.Create.Table;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Create
{
    public class CreateBuilder : ICreateBuilder
    {
        private readonly IMigrationContext _context;

        public CreateBuilder(IMigrationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public IExecutableBuilder Table<TDto>(bool withoutKeysAndIndexes = false)
        {
            return new CreateTableOfDtoBuilder(_context) { TypeOfDto = typeof(TDto), WithoutKeysAndIndexes = withoutKeysAndIndexes };
        }

        /// <inheritdoc />
        public IExecutableBuilder KeysAndIndexes<TDto>()
        {
            return new CreateKeysAndIndexesBuilder(_context) { TypeOfDto = typeof(TDto) };
        }

        /// <inheritdoc />
        public IExecutableBuilder KeysAndIndexes(Type typeOfDto)
        {
            return new CreateKeysAndIndexesBuilder(_context) { TypeOfDto = typeOfDto };
        }

        /// <inheritdoc />
        public ICreateTableWithColumnBuilder Table(string tableName)
        {
            var expression = new CreateTableExpression(_context) { TableName = tableName };
            return new CreateTableBuilder(_context, expression);
        }

        /// <inheritdoc />
        public ICreateColumnOnTableBuilder Column(string columnName)
        {
            var expression = new CreateColumnExpression(_context) { Column = { Name = columnName } };
            return new CreateColumnBuilder(_context, expression);
        }

        /// <inheritdoc />
        public ICreateForeignKeyFromTableBuilder ForeignKey()
        {
            var expression = new CreateForeignKeyExpression(_context);
            return new CreateForeignKeyBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateForeignKeyFromTableBuilder ForeignKey(string foreignKeyName)
        {
            var expression = new CreateForeignKeyExpression(_context) { ForeignKey = { Name = foreignKeyName } };
            return new CreateForeignKeyBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateIndexForTableBuilder Index()
        {
            var expression = new CreateIndexExpression(_context);
            return new CreateIndexBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateIndexForTableBuilder Index(string indexName)
        {
            var expression = new CreateIndexExpression(_context) { Index = { Name = indexName } };
            return new CreateIndexBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder PrimaryKey() => PrimaryKey(true);

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder PrimaryKey(bool clustered)
        {
            var expression = new CreateConstraintExpression(_context, ConstraintType.PrimaryKey);
            expression.Constraint.IsPrimaryKeyClustered = clustered;
            return new CreateConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder PrimaryKey(string primaryKeyName) => PrimaryKey(primaryKeyName, true);

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder PrimaryKey(string primaryKeyName, bool clustered)
        {
            var expression = new CreateConstraintExpression(_context, ConstraintType.PrimaryKey);
            expression.Constraint.ConstraintName = primaryKeyName;
            expression.Constraint.IsPrimaryKeyClustered = clustered;
            return new CreateConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder UniqueConstraint()
        {
            var expression = new CreateConstraintExpression(_context, ConstraintType.Unique);
            return new CreateConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder UniqueConstraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context, ConstraintType.Unique);
            expression.Constraint.ConstraintName = constraintName;
            return new CreateConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder Constraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context, ConstraintType.NonUnique);
            expression.Constraint.ConstraintName = constraintName;
            return new CreateConstraintBuilder(expression);
        }
    }
}

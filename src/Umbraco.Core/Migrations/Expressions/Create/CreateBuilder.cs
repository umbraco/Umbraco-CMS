using System;
using NPoco;
using Umbraco.Core.Migrations.Expressions.Common.Expressions;
using Umbraco.Core.Migrations.Expressions.Create.Column;
using Umbraco.Core.Migrations.Expressions.Create.Constraint;
using Umbraco.Core.Migrations.Expressions.Create.Expressions;
using Umbraco.Core.Migrations.Expressions.Create.ForeignKey;
using Umbraco.Core.Migrations.Expressions.Create.Index;
using Umbraco.Core.Migrations.Expressions.Create.Table;
using Umbraco.Core.Migrations.Expressions.Execute.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Expressions.Create
{
    /// <summary>
    /// Implements <see cref="ICreateBuilder"/>.
    /// </summary>
    public class CreateBuilder : ICreateBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public CreateBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        private ISqlSyntaxProvider SqlSyntax => _context.Database.SqlContext.SqlSyntax;

        /// <inheritdoc />
        public void Table<TDto>(bool withoutKeysAndIndexes = false)
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(typeof(TDto), SqlSyntax);

            ExecuteSql(SqlSyntax.Format(tableDefinition));
            if (withoutKeysAndIndexes)
                return;

            ExecuteSql(SqlSyntax.FormatPrimaryKey(tableDefinition));
            foreach (var sql in SqlSyntax.Format(tableDefinition.ForeignKeys))
                ExecuteSql(sql);
            foreach (var sql in SqlSyntax.Format(tableDefinition.Indexes))
                ExecuteSql(sql);
        }

        /// <inheritdoc />
        public void KeysAndIndexes<TDto>()
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(typeof(TDto), SqlSyntax);

            ExecuteSql(SqlSyntax.FormatPrimaryKey(tableDefinition));
            foreach (var sql in SqlSyntax.Format(tableDefinition.Indexes))
                ExecuteSql(sql);
            foreach (var sql in SqlSyntax.Format(tableDefinition.ForeignKeys))
                ExecuteSql(sql);
        }

        /// <inheritdoc />
        public void KeysAndIndexes(Type typeOfDto)
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(typeOfDto, SqlSyntax);

            ExecuteSql(SqlSyntax.FormatPrimaryKey(tableDefinition));
            foreach (var sql in SqlSyntax.Format(tableDefinition.Indexes))
                ExecuteSql(sql);
            foreach (var sql in SqlSyntax.Format(tableDefinition.ForeignKeys))
                ExecuteSql(sql);
        }

        private void ExecuteSql(string sql)
        {
            var expression = new ExecuteSqlStatementExpression(_context, _supportedDatabaseTypes) { SqlStatement = sql };
            expression.Execute();
        }

        /// <inheritdoc />
        public ICreateTableWithColumnBuilder Table(string tableName)
        {
            var expression = new CreateTableExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            return new CreateTableBuilder(_context, _supportedDatabaseTypes, expression);
        }

        /// <inheritdoc />
        public ICreateColumnOnTableSyntax Column(string columnName)
        {
            var expression = new CreateColumnExpression(_context, _supportedDatabaseTypes) { Column = { Name = columnName } };
            return new CreateColumnBuilder(_context, _supportedDatabaseTypes, expression);
        }

        /// <inheritdoc />
        public ICreateForeignKeyFromTableBuilder ForeignKey()
        {
            var expression = new CreateForeignKeyExpression(_context, _supportedDatabaseTypes);
            return new CreateForeignKeyBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateForeignKeyFromTableBuilder ForeignKey(string foreignKeyName)
        {
            var expression = new CreateForeignKeyExpression(_context, _supportedDatabaseTypes) { ForeignKey = { Name = foreignKeyName } };
            return new CreateForeignKeyBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateIndexForTableBuilder Index()
        {
            var expression = new CreateIndexExpression(_context, _supportedDatabaseTypes);
            return new CreateIndexBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateIndexForTableBuilder Index(string indexName)
        {
            var expression = new CreateIndexExpression(_context, _supportedDatabaseTypes) { Index = { Name = indexName } };
            return new CreateIndexBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder PrimaryKey()
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.PrimaryKey);
            return new CreateConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder PrimaryKey(string primaryKeyName)
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.PrimaryKey);
            expression.Constraint.ConstraintName = primaryKeyName;
            return new CreateConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder UniqueConstraint()
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.Unique);
            return new CreateConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder UniqueConstraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.Unique);
            expression.Constraint.ConstraintName = constraintName;
            return new CreateConstraintBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableBuilder Constraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(_context, _supportedDatabaseTypes, ConstraintType.NonUnique);
            expression.Constraint.ConstraintName = constraintName;
            return new CreateConstraintBuilder(expression);
        }
    }
}

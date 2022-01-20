﻿using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index
{
    public class CreateIndexBuilder : ExpressionBuilderBase<CreateIndexExpression>,
        ICreateIndexForTableBuilder,
        ICreateIndexOnColumnBuilder,
        ICreateIndexColumnOptionsBuilder,
        ICreateIndexOptionsBuilder
    {
        public CreateIndexBuilder(CreateIndexExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void Do() => Expression.Execute();

        public IndexColumnDefinition CurrentColumn { get; set; }

        /// <inheritdoc />
        public ICreateIndexOnColumnBuilder OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public ICreateIndexColumnOptionsBuilder OnColumn(string columnName)
        {
            CurrentColumn = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(CurrentColumn);
            return this;
        }

        /// <inheritdoc />
        public ICreateIndexOptionsBuilder WithOptions()
        {
            return this;
        }

        /// <inheritdoc />
        public ICreateIndexOnColumnBuilder Ascending()
        {
            CurrentColumn.Direction = Direction.Ascending;
            return this;
        }

        /// <inheritdoc />
        public ICreateIndexOnColumnBuilder Descending()
        {
            CurrentColumn.Direction = Direction.Descending;
            return this;
        }

        /// <inheritdoc />
        ICreateIndexOnColumnBuilder ICreateIndexColumnOptionsBuilder.Unique()
        {
            Expression.Index.IndexType = IndexTypes.UniqueNonClustered;
            return this;
        }

        /// <inheritdoc />
        public ICreateIndexOnColumnBuilder NonClustered()
        {
            Expression.Index.IndexType = IndexTypes.NonClustered;
            return this;
        }

        /// <inheritdoc />
        public ICreateIndexOnColumnBuilder Clustered()
        {
           Expression.Index.IndexType = IndexTypes.Clustered;
           return this;
        }

        /// <inheritdoc />
        ICreateIndexOnColumnBuilder ICreateIndexOptionsBuilder.Unique()
        {
            Expression.Index.IndexType = IndexTypes.UniqueNonClustered;
            return this;
        }
    }
}

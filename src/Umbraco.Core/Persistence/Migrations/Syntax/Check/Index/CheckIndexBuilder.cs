﻿using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Index
{
    public class CheckIndexBuilder : ExpressionBuilderBase<CheckIndexExpression>, ICheckIndexSyntax, ICheckIndexForTableSyntax, ICheckIndexOptionSyntax
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;
        private readonly ISqlSyntaxProvider _sqlSyntax;

        public CheckIndexBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckIndexExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public ICheckIndexOptionSyntax WithColumn(string columnName)
        {
            Expression.ColumnNames.Add(columnName);

            return this;
        }

        public ICheckIndexOptionSyntax WithColumns(string[] columnNames)
        {
            foreach (var columnName in columnNames)
                Expression.ColumnNames.Add(columnName);

            return this;
        }

        public bool Exists()
        {
            var indexes = _sqlSyntax.GetDefinedIndexesDefinitions(_context.Database);
            var foundIndexes = indexes.Where(x => x.IndexName.InvariantEquals(Expression.IndexName));

            if (string.IsNullOrWhiteSpace(Expression.TableName) == false)
            {
                foundIndexes = foundIndexes.Where(x => x.TableName.InvariantEquals(Expression.TableName));
            }
            
            if (Expression.Unique.HasValue)
            {
                foundIndexes = foundIndexes.Where(x => x.IsUnique.Equals(Expression.Unique.Value));
            }

            if (Expression.ColumnNames.Any())
            {
                return Expression.ColumnNames.All(x =>
                                                foundIndexes.Any(c => c.ColumnName.InvariantEquals(x)
                                             ));
            }

            return foundIndexes.Any();
        }

        public ICheckExistsSyntax NotUnique()
        {
            Expression.Unique = false;

            return this;
        }

        public ICheckIndexForTableSyntax OnTable(string tableName)
        {
            Expression.TableName = tableName;

            return this;
        }

        public ICheckExistsSyntax Unique()
        {
            Expression.Unique = true;

            return this;
        }
    }
}

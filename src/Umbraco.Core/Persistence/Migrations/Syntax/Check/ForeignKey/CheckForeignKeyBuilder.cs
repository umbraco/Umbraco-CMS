using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public class CheckForeignKeyBuilder : ExpressionBuilderBase<CheckForeignKeyExpression>, ICheckForeignKeySyntax
    {
        protected readonly IMigrationContext _context;
        protected readonly DatabaseProviders[] _databaseProviders;
        protected readonly ISqlSyntaxProvider _sqlSyntax;

        public CheckForeignKeyBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckForeignKeyExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public bool Exists()
        {
            var foundForeignKeys = _sqlSyntax.GetForeignKeys(_context.Database);
            var hasPrimaryColumnNames = Expression.PrimaryColumnNames.Any();
            var hasForeignColumnNames = Expression.ForeignColumnNames.Any();

            if (string.IsNullOrWhiteSpace(Expression.ForeignKeyName) == false)
            {
                foundForeignKeys = foundForeignKeys.Where(x => x.Item5.InvariantEquals(Expression.ForeignKeyName));
            }

            if (string.IsNullOrWhiteSpace(Expression.ForeignTableName) == false)
            {
                foundForeignKeys = foundForeignKeys.Where(x => x.Item1.InvariantEquals(Expression.ForeignTableName));
            }

            if (string.IsNullOrWhiteSpace(Expression.PrimaryTableName) == false)
            {
                foundForeignKeys = foundForeignKeys.Where(x => x.Item3.InvariantEquals(Expression.PrimaryTableName));
            }

            if (hasForeignColumnNames && hasPrimaryColumnNames)
            {
                return AllForeignColumnsMatch(foundForeignKeys)
                    && AllPrimaryColumnsMatch(foundForeignKeys);
            }
            else if (hasForeignColumnNames)
            {
                return AllForeignColumnsMatch(foundForeignKeys);
            }
            else if (hasPrimaryColumnNames)
            {
                return AllPrimaryColumnsMatch(foundForeignKeys);
            }

            return foundForeignKeys.Any();
        }

        private bool AllForeignColumnsMatch(IEnumerable<Tuple<string, string, string, string, string>> foundForeignKeys)
        {
            return AllColumnsMatch(Expression.ForeignColumnNames, foundForeignKeys.Select(x => x.Item2));
        }
        private bool AllPrimaryColumnsMatch(IEnumerable<Tuple<string, string, string, string, string>> foundForeignKeys)
        {
            return AllColumnsMatch(Expression.PrimaryColumnNames, foundForeignKeys.Select(x => x.Item4));
        }
        private bool AllColumnsMatch(ICollection<string> columnNames, IEnumerable<string> foundColumnNames)
        {
            return columnNames.All(x => foundColumnNames.InvariantContains(x));
        }

        public ICheckForeignKeyForFromTableSyntax FromTable(string tableName)
        {
            Expression.ForeignTableName = tableName;

            return new CheckForeignKeyForFromTableBuilder(_context, _databaseProviders, _sqlSyntax, Expression);
        }

        public ICheckForeignKeyForToTableSyntax ToTable(string tableName)
        {
            Expression.PrimaryTableName = tableName;

            return new CheckForeignKeyForToTableBuilder(_context, _databaseProviders, _sqlSyntax, Expression);
        }
    }
}

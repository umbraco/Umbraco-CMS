﻿using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions
{
    public class CheckConstraintExpression : MigrationExpressionBase
    {
        public CheckConstraintExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) : base(current, databaseProviders, sqlSyntax)
        {
        }

        public string ColumnName { get; set; }
        public string ConstraintName { get; set; }
        public string TableName { get; set; }
    }
}

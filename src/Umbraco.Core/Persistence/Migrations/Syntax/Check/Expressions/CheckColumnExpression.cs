﻿using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions
{
    public class CheckColumnExpression : MigrationExpressionBase
    {
        public CheckColumnExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) : base(current, databaseProviders, sqlSyntax)
        {
        }

        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }
    }
}

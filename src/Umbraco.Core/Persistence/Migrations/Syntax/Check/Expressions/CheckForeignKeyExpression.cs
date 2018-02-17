﻿using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions
{
    public class CheckForeignKeyExpression : MigrationExpressionBase
    {
        public CheckForeignKeyExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) : base(current, databaseProviders, sqlSyntax)
        {
        }

        public string ForeignKeyName { get; set; }
        public string ForeignTableName { get; set; }
        public string PrimaryTableName { get; set; }
        public string[] ForeignColumnNames { get; set; }
        public string[] PrimaryColumnNames { get; set; }
    }
}

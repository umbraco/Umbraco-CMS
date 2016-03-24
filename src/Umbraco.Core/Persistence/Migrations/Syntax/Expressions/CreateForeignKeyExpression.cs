﻿using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateForeignKeyExpression : MigrationExpressionBase
    {
        public CreateForeignKeyExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, ForeignKeyDefinition fkDef)
            : base(sqlSyntax, current, databaseProviders)
        {
            ForeignKey = fkDef;
        }

        public CreateForeignKeyExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(sqlSyntax, current, databaseProviders)
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public ForeignKeyDefinition ForeignKey { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return SqlSyntax.Format(ForeignKey);
        }
    }
}
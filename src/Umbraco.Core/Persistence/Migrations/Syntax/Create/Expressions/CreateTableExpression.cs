﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions
{
    public class CreateTableExpression : MigrationExpressionBase
    {


        public CreateTableExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(sqlSyntax, current, databaseProviders)
        {
            Columns = new List<ColumnDefinition>();
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual IList<ColumnDefinition> Columns { get; set; }

        public override string ToString()
        {
            var table = new TableDefinition { Name = TableName, SchemaName = SchemaName, Columns = Columns };

            return string.Format(SqlSyntax.Format(table));
        }
    }
}
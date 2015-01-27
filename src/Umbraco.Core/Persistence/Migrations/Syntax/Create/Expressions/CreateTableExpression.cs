using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions
{
    public class CreateTableExpression : MigrationExpressionBase
    {
        [Obsolete("Use the other constructors specifying an ISqlSyntaxProvider instead")]
        public CreateTableExpression()
        {
            Columns = new List<ColumnDefinition>();
        }

        [Obsolete("Use the other constructors specifying an ISqlSyntaxProvider instead")]
        public CreateTableExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders)
            : base(current, databaseProviders)
        {
            Columns = new List<ColumnDefinition>();
        }

        public CreateTableExpression(ISqlSyntaxProvider sqlSyntax)
            : base(sqlSyntax)
        {
            Columns = new List<ColumnDefinition>();
        }

        public CreateTableExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
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
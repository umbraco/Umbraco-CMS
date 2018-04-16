using System.Collections.Generic;
using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Create.Expressions
{
    public class CreateTableExpression : MigrationExpressionBase
    {
        public CreateTableExpression(IMigrationContext context)
            : base(context)
        {
            Columns = new List<ColumnDefinition>();
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual IList<ColumnDefinition> Columns { get; set; }

        protected override string GetSql()
        {
            var table = new TableDefinition { Name = TableName, SchemaName = SchemaName, Columns = Columns };

            return string.Format(SqlSyntax.Format(table));
        }
    }
}

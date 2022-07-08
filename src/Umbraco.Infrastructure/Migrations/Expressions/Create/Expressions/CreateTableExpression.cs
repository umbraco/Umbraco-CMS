using System.Collections.Generic;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions
{
    public class CreateTableExpression : MigrationExpressionBase
    {
        public CreateTableExpression(IMigrationContext context)
            : base(context)
        {
            Columns = new List<ColumnDefinition>();
        }

        public virtual string SchemaName { get; set; } = null!;
        public virtual string TableName { get; set; } = null!;
        public virtual IList<ColumnDefinition> Columns { get; set; }

        protected override string GetSql()
        {
            var table = new TableDefinition { Name = TableName, SchemaName = SchemaName, Columns = Columns };

            return string.Format(SqlSyntax.Format(table));
        }
    }
}

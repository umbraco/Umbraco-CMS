using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteColumnExpression : IMigrationExpression
    {
        public DeleteColumnExpression()
        {
            ColumnNames = new List<string>();
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public ICollection<string> ColumnNames { get; set; }

        public override string ToString()
        {
            //TODO Change to use sql syntax provider
            return base.ToString() + TableName + " " + ColumnNames.Aggregate((a, b) => a + ", " + b);
        }
    }
}
using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateIndexExpression : IMigrationExpression
    {
        public CreateIndexExpression()
        {
            Index = new IndexDefinition();
        }

        public virtual IndexDefinition Index { get; set; }

        public override string ToString()
        {
            return base.ToString() + Index.TableName + " (" + string.Join(", ", Index.Columns.Select(x => x.Name).ToArray()) + ")";
        }
    }
}
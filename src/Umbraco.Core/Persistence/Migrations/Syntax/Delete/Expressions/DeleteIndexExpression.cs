using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteIndexExpression : IMigrationExpression
    {
        public DeleteIndexExpression()
        {
            Index = new IndexDefinition();
        }

        public virtual IndexDefinition Index { get; set; }

        public override string ToString()
        {
            //TODO Change to use sql syntax provider
            return base.ToString() + Index.TableName + " (" + string.Join(", ", Index.Columns.Select(x => x.Name).ToArray()) + ")";
        }
    }
}
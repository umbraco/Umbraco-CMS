using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteForeignKeyExpression : IMigrationExpression
    {
        public DeleteForeignKeyExpression()
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public virtual ForeignKeyDefinition ForeignKey { get; set; }

        public override string ToString()
        {
            //TODO Change to use sql syntax provider
            return base.ToString() + ForeignKey.Name + " "
                + ForeignKey.ForeignTable + " (" + string.Join(", ", ForeignKey.ForeignColumns.ToArray()) + ") "
                + ForeignKey.PrimaryTable + " (" + string.Join(", ", ForeignKey.PrimaryColumns.ToArray()) + ")";
        }
    }
}
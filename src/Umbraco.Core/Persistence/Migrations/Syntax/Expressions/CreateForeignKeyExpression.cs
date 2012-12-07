using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateForeignKeyExpression : IMigrationExpression
    {
        public CreateForeignKeyExpression()
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public virtual ForeignKeyDefinition ForeignKey { get; set; }

        public override string ToString()
        {
            return base.ToString() +
                    string.Format("{0} {1}({2}) {3}({4})",
                                ForeignKey.Name,
                                ForeignKey.ForeignTable,
                                string.Join(", ", ForeignKey.ForeignColumns.ToArray()),
                                ForeignKey.PrimaryTable,
                                string.Join(", ", ForeignKey.PrimaryColumns.ToArray()));
        }
    }
}
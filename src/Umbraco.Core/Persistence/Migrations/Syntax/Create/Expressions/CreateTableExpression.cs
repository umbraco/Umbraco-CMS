using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions
{
    public class CreateTableExpression : IMigrationExpression
    {
        public CreateTableExpression()
        {
            Columns = new List<ColumnDefinition>();
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual IList<ColumnDefinition> Columns { get; set; }

        public override string ToString()
        {
            //TODO replace with sql syntax provider
            return base.ToString() + TableName;
        }
    }
}
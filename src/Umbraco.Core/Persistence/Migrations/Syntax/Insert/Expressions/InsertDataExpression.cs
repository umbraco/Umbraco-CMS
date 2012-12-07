using System.Collections.Generic;
using Umbraco.Core.Persistence.Migrations.Model;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert.Expressions
{
    public class InsertDataExpression : IMigrationExpression
    {
        private readonly List<InsertionDataDefinition> _rows = new List<InsertionDataDefinition>();
        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public List<InsertionDataDefinition> Rows
        {
            get { return _rows; }
        }

        public override string ToString()
        {
            //TODO implement the use of sql syntax provider?

            return string.Empty;
        }
    }
}
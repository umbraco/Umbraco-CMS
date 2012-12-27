using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert.Expressions
{
    public class InsertDataExpression : MigrationExpressionBase
    {
        private readonly List<InsertionDataDefinition> _rows = new List<InsertionDataDefinition>();

        public InsertDataExpression()
        {
        }

        public InsertDataExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
        }

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
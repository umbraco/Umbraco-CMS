using System.Collections.Generic;
using Umbraco.Core.Persistence.Migrations.Model;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteDataExpression : IMigrationExpression
    {
        private readonly List<DeletionDataDefinition> _rows = new List<DeletionDataDefinition>();
        public virtual string SchemaName { get; set; }
        public string TableName { get; set; }
        public virtual bool IsAllRows { get; set; }

        public List<DeletionDataDefinition> Rows
        {
            get { return _rows; }
        }
    }
}
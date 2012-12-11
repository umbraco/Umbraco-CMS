using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Update.Expressions
{
    public class UpdateDataExpression : IMigrationExpression
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public List<KeyValuePair<string, object>> Set { get; set; }
        public List<KeyValuePair<string, object>> Where { get; set; }
        public bool IsAllRows { get; set; }
    }
}
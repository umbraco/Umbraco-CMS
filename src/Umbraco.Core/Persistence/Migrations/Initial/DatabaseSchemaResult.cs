using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Migrations.Initial
{
    public class DatabaseSchemaResult
    {
        public DatabaseSchemaResult()
        {
            Errors = new Dictionary<string, string>();
            Successes = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Errors { get; set; }

        public IDictionary<string, string> Successes { get; set; }
    }
}
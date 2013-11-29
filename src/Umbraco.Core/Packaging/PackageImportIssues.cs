using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Packaging
{
    public class PackageImportIssues
    {
        public bool ContainsUnsecureFiles { get { return UnsecureFiles != null && UnsecureFiles.Any(); } }
        public IEnumerable<string> UnsecureFiles { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ConflictingMacroAliases { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ConflictingTemplateAliases { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ConflictingStylesheetNames { get; set; }
        
    }
}
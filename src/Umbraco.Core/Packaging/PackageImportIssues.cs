using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    public class PackageImportIssues
    {
        public bool ContainsUnsecureFiles { get { return UnsecureFiles != null && UnsecureFiles.Any(); } }
        public IFileInPackageInfo[] UnsecureFiles { get; set; }
        public IMacro[] ConflictingMacroAliases { get; set; }
        public ITemplate[] ConflictingTemplateAliases { get; set; }
        public IStylesheet[] ConflictingStylesheetNames { get; set; }
    }
}
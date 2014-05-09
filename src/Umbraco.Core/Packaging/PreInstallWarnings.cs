using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    public class PreInstallWarnings
    {
        public IFileInPackageInfo[] UnsecureFiles { get; set; }
        public IMacro[] ConflictingMacroAliases { get; set; }
        public ITemplate[] ConflictingTemplateAliases { get; set; }
        public IStylesheet[] ConflictingStylesheetNames { get; set; }
    }
}
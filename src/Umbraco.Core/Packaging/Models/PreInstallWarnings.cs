using Umbraco.Core.Models;

namespace Umbraco.Core.Packaging.Models
{
    public class PreInstallWarnings
    {
        public IFileInPackageInfo[] UnsecureFiles { get; set; }
        public IMacro[] ConflictingMacroAliases { get; set; }
        public ITemplate[] ConflictingTemplateAliases { get; set; }
        public IFile[] ConflictingStylesheetNames { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Core.Packaging.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class PreInstallWarnings
    {
        public KeyValuePair<string, string>[] UnsecureFiles { get; set; }
        public KeyValuePair<string, string>[] FilesReplaced { get; set; }
        public IMacro[] ConflictingMacroAliases { get; set; }
        public ITemplate[] ConflictingTemplateAliases { get; set; }
        public IFile[] ConflictingStylesheetNames { get; set; }
        public string[] AssembliesWithLegacyPropertyEditors { get; set; }
    }
}
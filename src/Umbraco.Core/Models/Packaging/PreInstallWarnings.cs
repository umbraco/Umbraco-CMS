using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Packaging
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class PreInstallWarnings
    {
        public KeyValuePair<string, string>[] UnsecureFiles { get; set; }
        public KeyValuePair<string, string>[] FilesReplaced { get; set; }
        public IEnumerable<IMacro> ConflictingMacroAliases { get; set; }
        public IEnumerable<ITemplate> ConflictingTemplateAliases { get; set; }
        public IEnumerable<IFile> ConflictingStylesheetNames { get; set; }
    }
}

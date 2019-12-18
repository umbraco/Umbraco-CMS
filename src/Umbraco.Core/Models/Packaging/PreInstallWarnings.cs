using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Packaging
{
    public class PreInstallWarnings
    {
        public IEnumerable<string> UnsecureFiles { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> FilesReplaced { get; set; } = Enumerable.Empty<string>();

        // TODO: Shouldn't we detect other conflicting entities too ?
        public IEnumerable<IMacro> ConflictingMacros { get; set; } = Enumerable.Empty<IMacro>();
        public IEnumerable<ITemplate> ConflictingTemplates { get; set; } = Enumerable.Empty<ITemplate>();
        public IEnumerable<IFile> ConflictingStylesheets { get; set; } = Enumerable.Empty<IFile>();
    }
}

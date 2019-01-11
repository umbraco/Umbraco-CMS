using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Packaging
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class UninstallationSummary
    {
        public IPackageInfo MetaData { get; set; }
        public IEnumerable<IDataType> DataTypesUninstalled { get; set; } = Enumerable.Empty<IDataType>();
        public IEnumerable<ILanguage> LanguagesUninstalled { get; set; } = Enumerable.Empty<ILanguage>();
        public IEnumerable<IDictionaryItem> DictionaryItemsUninstalled { get; set; } = Enumerable.Empty<IDictionaryItem>();
        public IEnumerable<IMacro> MacrosUninstalled { get; set; } = Enumerable.Empty<IMacro>();
        public IEnumerable<string> FilesUninstalled { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<ITemplate> TemplatesUninstalled { get; set; } = Enumerable.Empty<ITemplate>();
        public IEnumerable<IContentType> ContentTypesUninstalled { get; set; } = Enumerable.Empty<IContentType>();
        public IEnumerable<IFile> StylesheetsUninstalled { get; set; } = Enumerable.Empty<IFile>();
        public IEnumerable<IContent> ContentUninstalled { get; set; } = Enumerable.Empty<IContent>();
        public bool PackageUninstalled { get; set; }
    }
}

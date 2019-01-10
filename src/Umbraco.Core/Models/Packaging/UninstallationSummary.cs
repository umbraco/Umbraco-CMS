using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Packaging
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class UninstallationSummary
    {
        public MetaData MetaData { get; set; }
        public IEnumerable<IDataType> DataTypesUninstalled { get; set; }
        public IEnumerable<ILanguage> LanguagesUninstalled { get; set; }
        public IEnumerable<IDictionaryItem> DictionaryItemsUninstalled { get; set; }
        public IEnumerable<IMacro> MacrosUninstalled { get; set; }
        public IEnumerable<string> FilesUninstalled { get; set; }
        public IEnumerable<ITemplate> TemplatesUninstalled { get; set; }
        public IEnumerable<IContentType> ContentTypesUninstalled { get; set; }
        public IEnumerable<IFile> StylesheetsUninstalled { get; set; }
        public IEnumerable<IContent> ContentUninstalled { get; set; }
        public bool PackageUninstalled { get; set; }
    }
}

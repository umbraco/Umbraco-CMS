using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Packaging
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class InstallationSummary
    {
        public MetaData MetaData { get; set; }
        public IEnumerable<IDataType> DataTypesInstalled { get; set; }
        public IEnumerable<ILanguage> LanguagesInstalled { get; set; }
        public IEnumerable<IDictionaryItem> DictionaryItemsInstalled { get; set; }
        public IEnumerable<IMacro> MacrosInstalled { get; set; }
        public IEnumerable<string> FilesInstalled { get; set; }
        public IEnumerable<ITemplate> TemplatesInstalled { get; set; }
        public IEnumerable<IContentType> ContentTypesInstalled { get; set; }
        public IEnumerable<IFile> StylesheetsInstalled { get; set; }
        public IEnumerable<IContent> ContentInstalled { get; set; }
        public IEnumerable<PackageAction> Actions { get; set; }
        public bool PackageInstalled { get; set; }
    }
}

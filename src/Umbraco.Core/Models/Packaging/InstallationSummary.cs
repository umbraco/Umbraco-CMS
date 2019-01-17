using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Packaging
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class InstallationSummary
    {
        public IPackageInfo MetaData { get; set; }
        public IEnumerable<IDataType> DataTypesInstalled { get; set; } = Enumerable.Empty<IDataType>();
        public IEnumerable<ILanguage> LanguagesInstalled { get; set; } = Enumerable.Empty<ILanguage>();
        public IEnumerable<IDictionaryItem> DictionaryItemsInstalled { get; set; } = Enumerable.Empty<IDictionaryItem>();
        public IEnumerable<IMacro> MacrosInstalled { get; set; } = Enumerable.Empty<IMacro>();
        public IEnumerable<string> FilesInstalled { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<ITemplate> TemplatesInstalled { get; set; } = Enumerable.Empty<ITemplate>();
        public IEnumerable<IContentType> DocumentTypesInstalled { get; set; } = Enumerable.Empty<IContentType>();
        public IEnumerable<IFile> StylesheetsInstalled { get; set; } = Enumerable.Empty<IFile>();
        public IEnumerable<IContent> ContentInstalled { get; set; } = Enumerable.Empty<IContent>();
        public IEnumerable<PackageAction> Actions { get; set; } = Enumerable.Empty<PackageAction>();
        public IEnumerable<string> ActionErrors { get; set; } = Enumerable.Empty<string>();
        
    }

}

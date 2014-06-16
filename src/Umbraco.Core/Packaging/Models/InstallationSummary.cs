using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Core.Packaging.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class InstallationSummary
    {
        public MetaData MetaData { get; set; }
        public IDataTypeDefinition[] DataTypesInstalled { get; set; }
        public ILanguage[] LanguagesInstalled { get; set; }
        public IDictionaryItem[] DictionaryItemsInstalled { get; set; }
        public IMacro[] MacrosInstalled { get; set; }
        public string[] FilesInstalled { get; set; }
        public ITemplate[] TemplatesInstalled { get; set; }
        public IContentType[] ContentTypesInstalled { get; set; }
        public IFile[] StylesheetsInstalled { get; set; }
        public IContent[] ContentInstalled { get; set; }
        public PackageAction[] Actions { get; set; }
    }
}
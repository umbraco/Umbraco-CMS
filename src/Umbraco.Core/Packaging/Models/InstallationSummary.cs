using System;
using System.Collections.Generic;
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
        public IEnumerable<KeyValuePair<string, bool>> FilesInstalled { get; set; }
        public ITemplate[] TemplatesInstalled { get; set; }
        public IContentType[] DocumentTypesInstalled { get; set; }
        public IStylesheet[] StylesheetsInstalled { get; set; }
        public IContent[] DocumentsInstalled { get; set; }
        public PackageAction[] Actions { get; set; }
    }
}
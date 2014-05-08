using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Packaging
{
    public class PackageInstallationSummary
    {
        public PackageMetaData MetaData { get; set; }
        public IDataTypeDefinition[] DataTypesInstalled { get; set; }
        public ILanguage[] LanguagesInstalled { get; set; }
        public IDictionaryItem[] DictionaryItemsInstalled { get; set; }
        public IMacro[] MacrosInstalled { get; set; }
        public IEnumerable<KeyValuePair<string, bool>> FilesInstalled { get;set;}
        public ITemplate[] TemplatesInstalled { get; set; }
        public IContentType[] DocumentTypesInstalled { get; set; }
        public IStylesheet[] StylesheetsInstalled { get; set; }
        public IContent[] DocumentsInstalled { get; set; }
        public IEnumerable<KeyValuePair<string, XElement>> PackageInstallActions { get; set; }
        public string PackageUninstallActions { get; set; }
    }
}
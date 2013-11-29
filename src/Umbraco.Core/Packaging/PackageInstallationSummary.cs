using System.Collections.Generic;
using System.Xml;

namespace Umbraco.Core.Packaging
{
    public class PackageInstallationSummary
    {
        public PackageMetaData MetaData { get; set; }
        public IEnumerable<int> DataTypesInstalled { get; set; }
        public IEnumerable<int> LanguagesInstalled { get; set; }
        public IEnumerable<int> DictionaryItemsInstalled { get; set; }
        public IEnumerable<int> MacrosInstalled { get; set; }
        public IEnumerable<KeyValuePair<string, bool>> FilesInstalled { get;set;}
        public IEnumerable<int> TemplatesInstalled { get; set; }
        public IEnumerable<int> DocumentTypesInstalled { get; set; }
        public IEnumerable<int> StylesheetsInstalled { get; set; }
        public IEnumerable<int> DocumentsInstalled { get; set; }
        public Dictionary<string, XmlNode> PackageInstallActions { get; set; }
        public string PackageUninstallActions { get; set; }
    }
}
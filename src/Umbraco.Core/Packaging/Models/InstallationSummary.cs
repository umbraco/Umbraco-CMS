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
        public Details<string>[] FilesInstalled { get; set; }
        public ITemplate[] TemplatesInstalled { get; set; }
        public IContentType[] DocumentTypesInstalled { get; set; }
        public IFile[] StylesheetsInstalled { get; set; }
        public IContent[] DocumentsInstalled { get; set; }
        public PackageAction[] Actions { get; set; }
    }

    [Serializable]
    [DataContract(IsReference = true)]
    public enum InstallStatus
    {
        Inserted,
        Overwridden
    }


    [Serializable]
    [DataContract(IsReference = true)]
    public class Details<TItem>
    {
        public InstallStatus Status { get; set; }
        public TItem Destination { get; set; }
        public TItem Source { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Packaging.Models
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

    internal static class UninstallationSummaryExtentions
    {
        public static UninstallationSummary InitEmpty(this UninstallationSummary summary)
        {
            summary.ContentUninstalled = new List<IContent>();
            summary.ContentTypesUninstalled = new List<IContentType>();
            summary.DataTypesUninstalled = new List<IDataType>();
            summary.DictionaryItemsUninstalled = new List<IDictionaryItem>();
            summary.FilesUninstalled = new List<string>();
            summary.LanguagesUninstalled = new List<ILanguage>();
            summary.MacrosUninstalled = new List<IMacro>();
            summary.MetaData = new MetaData();
            summary.TemplatesUninstalled = new List<ITemplate>();
            summary.PackageUninstalled = false;
            return summary;
        }
    }
}

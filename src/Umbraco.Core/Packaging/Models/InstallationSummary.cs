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
        public IEnumerable<IDataTypeDefinition> DataTypesInstalled { get; set; }
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
    
    internal static class InstallationSummaryExtentions
    {
        public static InstallationSummary InitEmpty(this InstallationSummary summary)
        {
            summary.Actions = new List<PackageAction>();
            summary.ContentInstalled = new List<IContent>();
            summary.ContentTypesInstalled = new List<IContentType>();
            summary.DataTypesInstalled = new List<IDataTypeDefinition>();
            summary.DictionaryItemsInstalled = new List<IDictionaryItem>();
            summary.FilesInstalled = new List<string>();
            summary.LanguagesInstalled = new List<ILanguage>();
            summary.MacrosInstalled = new List<IMacro>();
            summary.MetaData = new MetaData();
            summary.TemplatesInstalled = new List<ITemplate>();
            summary.PackageInstalled = false;
            return summary;
        }
    }
}
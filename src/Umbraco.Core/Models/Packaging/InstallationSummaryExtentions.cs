using System.Collections.Generic;

namespace Umbraco.Core.Models.Packaging
{
    internal static class InstallationSummaryExtentions
    {
        public static InstallationSummary InitEmpty(this InstallationSummary summary)
        {
            summary.Actions = new List<PackageAction>();
            summary.ContentInstalled = new List<IContent>();
            summary.ContentTypesInstalled = new List<IContentType>();
            summary.DataTypesInstalled = new List<IDataType>();
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
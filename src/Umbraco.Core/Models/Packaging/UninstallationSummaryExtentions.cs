using System.Collections.Generic;

namespace Umbraco.Core.Models.Packaging
{
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

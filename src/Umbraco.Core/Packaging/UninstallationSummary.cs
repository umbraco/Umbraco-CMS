using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging
{
    // TODO: Probably can be killed
    [Serializable]
    [DataContract(IsReference = true)]
    public class UninstallationSummary
    {
        public IEnumerable<IDataType> DataTypesUninstalled { get; set; } = Enumerable.Empty<IDataType>();
        public IEnumerable<ILanguage> LanguagesUninstalled { get; set; } = Enumerable.Empty<ILanguage>();
        public IEnumerable<IDictionaryItem> DictionaryItemsUninstalled { get; set; } = Enumerable.Empty<IDictionaryItem>();
        public IEnumerable<IMacro> MacrosUninstalled { get; set; } = Enumerable.Empty<IMacro>();
        public IEnumerable<ITemplate> TemplatesUninstalled { get; set; } = Enumerable.Empty<ITemplate>();
        public IEnumerable<IContentType> DocumentTypesUninstalled { get; set; } = Enumerable.Empty<IContentType>();
        public IEnumerable<IFile> StylesheetsUninstalled { get; set; } = Enumerable.Empty<IFile>();

    }
}

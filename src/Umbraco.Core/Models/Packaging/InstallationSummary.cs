using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.IO;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models.Packaging
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class InstallationSummary
    {
        public IPackageInfo MetaData { get; set; }
        public IEnumerable<IDataType> DataTypesInstalled { get; set; } = Enumerable.Empty<IDataType>();
        public IEnumerable<ILanguage> LanguagesInstalled { get; set; } = Enumerable.Empty<ILanguage>();
        public IEnumerable<IDictionaryItem> DictionaryItemsInstalled { get; set; } = Enumerable.Empty<IDictionaryItem>();
        public IEnumerable<IMacro> MacrosInstalled { get; set; } = Enumerable.Empty<IMacro>();
        public IEnumerable<string> FilesInstalled { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<ITemplate> TemplatesInstalled { get; set; } = Enumerable.Empty<ITemplate>();
        public IEnumerable<IContentType> DocumentTypesInstalled { get; set; } = Enumerable.Empty<IContentType>();
        public IEnumerable<IFile> StylesheetsInstalled { get; set; } = Enumerable.Empty<IFile>();
        public IEnumerable<IContent> ContentInstalled { get; set; } = Enumerable.Empty<IContent>();
        public IEnumerable<PackageAction> Actions { get; set; } = Enumerable.Empty<PackageAction>();
        public bool PackageInstalled { get; set; }

        //public static InstallationSummary FromPackageDefinition(PackageDefinition def, IContentTypeService contentTypeService, IDataTypeService dataTypeService, IFileService fileService, ILocalizationService localizationService, IMacroService macroService)
        //{
        //    var macros = TryGetIntegerIds(def.Macros).Select(macroService.GetById).ToList();
        //    var templates = TryGetIntegerIds(def.Templates).Select(fileService.GetTemplate).ToList();
        //    var contentTypes = TryGetIntegerIds(def.DocumentTypes).Select(contentTypeService.Get).ToList(); // fixme - media types?
        //    var dataTypes = TryGetIntegerIds(def.DataTypes).Select(dataTypeService.GetDataType).ToList();
        //    var dictionaryItems = TryGetIntegerIds(def.DictionaryItems).Select(localizationService.GetDictionaryItemById).ToList();
        //    var languages = TryGetIntegerIds(def.Languages).Select(localizationService.GetLanguageById).ToList();

        //    for (var i = 0; i < def.Files.Count; i++)
        //    {
        //        var filePath = def.Files[i];
        //        def.Files[i] = filePath.GetRelativePath();
        //    }

        //    return new InstallationSummary
        //    {
        //        ContentTypesInstalled = contentTypes,
        //        DataTypesInstalled = dataTypes,
        //        DictionaryItemsInstalled = dictionaryItems,
        //        FilesInstalled = def.Files,
        //        LanguagesInstalled = languages,
        //        MacrosInstalled = macros,
        //        MetaData = def,
        //        TemplatesInstalled = templates,
        //    };
        //}

        //private static IEnumerable<int> TryGetIntegerIds(IEnumerable<string> ids)
        //{
        //    var intIds = new List<int>();
        //    foreach (var id in ids)
        //    {
        //        if (int.TryParse(id, out var parsed))
        //            intIds.Add(parsed);
        //    }
        //    return intIds;
        //}
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Services;

namespace Umbraco.Web._Legacy.Packager.PackageInstance
{
    public class InstalledPackage
    {

        private int _saveHitCount = 0;

        public static InstalledPackage GetById(int id)
        {
            InstalledPackage pack = new InstalledPackage();
            pack.Data = data.Package(id, IOHelper.MapPath(Settings.InstalledPackagesSettings));
            return pack;
        }

        public static InstalledPackage MakeNew(string name)
        {
            InstalledPackage pack = new InstalledPackage();
            pack.Data = data.MakeNew(name, IOHelper.MapPath(Settings.InstalledPackagesSettings));
            return pack;
        }

        public void Save()
        {
#if DEBUG
            _saveHitCount++;
            Current.Logger.Info<InstalledPackage>("The InstalledPackage class save method has been hit {Total} times.", _saveHitCount);
#endif
            data.Save(this.Data, IOHelper.MapPath(Settings.InstalledPackagesSettings));
        }

        public static List<InstalledPackage> GetAllInstalledPackages()
        {

            List<InstalledPackage> val = new List<InstalledPackage>();

            foreach (Core.Models.Packaging.PackageDefinition pack in data.GetAllPackages(IOHelper.MapPath(Settings.InstalledPackagesSettings)))
            {
                InstalledPackage insPackage = new InstalledPackage();
                insPackage.Data = pack;
                val.Add(insPackage);
            }

            return val;
        }

        public Core.Models.Packaging.PackageDefinition Data { get; set; }

        public void Delete(int userId)
        {
            Current.Services.AuditService.Add(AuditType.PackagerUninstall, userId, -1, "Package", string.Format("Package '{0}' uninstalled. Package guid: {1}", Data.Name, Data.PackageId));
            Delete();
        }

        public void Delete()
        {
            data.Delete(this.Data.Id, IOHelper.MapPath(Settings.InstalledPackagesSettings));
        }



        /// <summary>
        /// Used internally for creating an InstallationSummary (used in new PackagingService) representation of this InstalledPackage object.
        /// </summary>
        /// <param name="contentTypeService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="fileService"></param>
        /// <param name="localizationService"></param>
        /// <param name="macroService"></param>
        /// <returns></returns>
        internal InstallationSummary GetInstallationSummary(IContentTypeService contentTypeService, IDataTypeService dataTypeService, IFileService fileService, ILocalizationService localizationService, IMacroService macroService)
        {
            var macros = TryGetIntegerIds(Data.Macros).Select(macroService.GetById).ToList();
            var templates = TryGetIntegerIds(Data.Templates).Select(fileService.GetTemplate).ToList();
            var contentTypes = TryGetIntegerIds(Data.DocumentTypes).Select(contentTypeService.Get).ToList(); // fixme - media types?
            var dataTypes = TryGetIntegerIds(Data.DataTypes).Select(dataTypeService.GetDataType).ToList();
            var dictionaryItems = TryGetIntegerIds(Data.DictionaryItems).Select(localizationService.GetDictionaryItemById).ToList();
            var languages = TryGetIntegerIds(Data.Languages).Select(localizationService.GetLanguageById).ToList();

            for (var i = 0; i < Data.Files.Count; i++)
            {
                var filePath = Data.Files[i];
                Data.Files[i] = filePath.GetRelativePath();
            }

            return new InstallationSummary
            {
                ContentTypesInstalled = contentTypes,
                DataTypesInstalled = dataTypes,
                DictionaryItemsInstalled = dictionaryItems,
                FilesInstalled = Data.Files,
                LanguagesInstalled = languages,
                MacrosInstalled = macros,
                MetaData = GetMetaData(),
                TemplatesInstalled = templates,
            };
        }

        internal MetaData GetMetaData()
        {
            return new MetaData()
            {
                AuthorName = Data.Author,
                AuthorUrl = Data.AuthorUrl,
                Control = Data.LoadControl,
                License = Data.License,
                LicenseUrl = Data.LicenseUrl,
                Name = Data.Name,
                Readme = Data.Readme,
                Url = Data.Url,
                Version = Data.Version
            };
        }

        private static IEnumerable<int> TryGetIntegerIds(IEnumerable<string> ids)
        {
            var intIds = new List<int>();
            foreach (var id in ids)
            {
                int parsed;
                if (int.TryParse(id, out parsed))
                    intIds.Add(parsed);
            }
            return intIds;
        }
    }
}

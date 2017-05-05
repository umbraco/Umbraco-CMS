using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Auditing;
using Umbraco.Core.Logging;
using Umbraco.Core.IO;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;

namespace umbraco.cms.businesslogic.packager {
    public class InstalledPackage
    {

        private int _saveHitCount = 0;

        public static InstalledPackage GetById(int id) {
            InstalledPackage pack = new InstalledPackage();
            pack.Data = data.Package(id, IOHelper.MapPath(Settings.InstalledPackagesSettings));
            return pack;
        }

        public static InstalledPackage GetByGuid(string packageGuid) {
            InstalledPackage pack = new InstalledPackage();
            pack.Data = data.Package(packageGuid, IOHelper.MapPath(Settings.InstalledPackagesSettings));
            return pack;
        }

        public static InstalledPackage MakeNew(string name) {
            InstalledPackage pack = new InstalledPackage();
            pack.Data = data.MakeNew(name, IOHelper.MapPath(Settings.InstalledPackagesSettings));
            pack.OnNew(EventArgs.Empty);
            return pack;
        }

        public void Save()
        {
#if DEBUG
            _saveHitCount++;
            LogHelper.Info<InstalledPackage>("The InstalledPackage class save method has been hit " + _saveHitCount + " times.");
#endif
            this.FireBeforeSave(EventArgs.Empty);
            data.Save(this.Data, IOHelper.MapPath(Settings.InstalledPackagesSettings));
            this.FireAfterSave(EventArgs.Empty);
        }

        public static List<InstalledPackage> GetAllInstalledPackages() {

            List<InstalledPackage> val = new List<InstalledPackage>();

            foreach (PackageInstance pack in data.GetAllPackages(IOHelper.MapPath(Settings.InstalledPackagesSettings)))
            {
                InstalledPackage insPackage = new InstalledPackage();
                insPackage.Data = pack;
                val.Add(insPackage);
            }

            return val;
        }

        private PackageInstance m_data;
        public PackageInstance Data {
            get { return m_data; }
            set { m_data = value; }
        }

        public void Delete(int userId)
        {
            Audit.Add(AuditTypes.PackagerUninstall, string.Format("Package '{0}' uninstalled. Package guid: {1}", Data.Name, Data.PackageGuid), userId, -1);
            Delete();
        }

        public void Delete() {
            this.FireBeforeDelete(EventArgs.Empty);
            data.Delete(this.Data.Id, IOHelper.MapPath(Settings.InstalledPackagesSettings));
            this.FireAfterDelete(EventArgs.Empty);
        }

        public static bool isPackageInstalled(string packageGuid) {
			try
			{
				if (data.GetFromGuid(packageGuid, IOHelper.MapPath(Settings.InstalledPackagesSettings), true) == null)
					return false;
				else
					return true;
			}
			catch (Exception ex)
			{
				LogHelper.Error<InstalledPackage>("An error occured in isPackagedInstalled", ex);
				return false;
			}
        }

        //EVENTS
        public delegate void SaveEventHandler(InstalledPackage sender, EventArgs e);
        public delegate void NewEventHandler(InstalledPackage sender, EventArgs e);
        public delegate void DeleteEventHandler(InstalledPackage sender, EventArgs e);

        /// <summary>
        /// Occurs when a macro is saved.
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(EventArgs e) {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(EventArgs e) {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(EventArgs e) {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(EventArgs e) {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(EventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
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
            var contentTypes = TryGetIntegerIds(Data.Documenttypes).Select(contentTypeService.GetContentType).ToList();
            var dataTypes = TryGetIntegerIds(Data.DataTypes).Select(dataTypeService.GetDataTypeDefinitionById).ToList();
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

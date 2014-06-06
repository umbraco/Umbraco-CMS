using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    internal class PackageInstallation : IPackageInstallation
    {
        private readonly IFileService _fileService;
        private readonly IMacroService _macroService;
        private readonly IPackagingService _packagingService;
        private IConflictingPackageContentFinder _conflictingPackageContentFinder;
        private readonly IPackageExtraction _packageExtraction;

        public PackageInstallation(IPackagingService packagingService, IMacroService macroService,
            IFileService fileService, IPackageExtraction packageExtraction)
        {
            if (packageExtraction != null) _packageExtraction = packageExtraction; 
            else throw new ArgumentNullException("packageExtraction");
            if (macroService != null) _macroService = macroService;
            else throw new ArgumentNullException("macroService");
            if (fileService != null) _fileService = fileService;
            else throw new ArgumentNullException("fileService");
            if (packagingService != null) _packagingService = packagingService;
            else throw new ArgumentNullException("packagingService");
        }


        public IConflictingPackageContentFinder ConflictingPackageContentFinder
        {
            private get
            {
                return _conflictingPackageContentFinder ??
                       (_conflictingPackageContentFinder = new ConflictingPackageContentFinder(_macroService, _fileService));
            }
            set
            {
                if (_conflictingPackageContentFinder != null)
                {
                    throw new PropertyConstraintException("This property already have a value");
                }
                _conflictingPackageContentFinder = value;
            }
        }



        private string _fullpathToRoot;
        public string FullpathToRoot
        {
            private get { return _fullpathToRoot ?? (_fullpathToRoot = GlobalSettings.FullpathToRoot); }
            set
            {

                if (_fullpathToRoot != null)
                {
                    throw new PropertyConstraintException("This property already have a value");
                }

                _fullpathToRoot = value;
            }
        }


        public MetaData GetMetaData(string packageFilePath)
        {
            try
            {
                XElement rootElement = GetConfigXmlElement(packageFilePath);
                return GetMetaData(rootElement);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFilePath, e);
            }
        }

        public PreInstallWarnings GetPreInstallWarnings(string packageFilePath)
        {
            try
            {
                XElement rootElement = GetConfigXmlElement(packageFilePath);
                return GetPreInstallWarnings(rootElement);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFilePath, e);
            }
        }

        public InstallationSummary InstallPackage(string packageFile, int userId)
        {
            XElement dataTypes;
            XElement languages;
            XElement dictionaryItems;
            XElement macroes;
            XElement files;
            XElement templates;
            XElement documentTypes;
            XElement styleSheets;
            XElement documentSet;
            XElement actions;
            MetaData metaData;
            InstallationSummary installationSummary;
            
            try
            {
                XElement rootElement = GetConfigXmlElement(packageFile);
                PackageSupportedCheck(rootElement);
                PackageStructureSanetyCheck(packageFile, rootElement);
                dataTypes = rootElement.Element(Constants.Packaging.DataTypesNodeName);
                languages = rootElement.Element(Constants.Packaging.LanguagesNodeName);
                dictionaryItems = rootElement.Element(Constants.Packaging.DictionaryItemsNodeName);
                macroes = rootElement.Element(Constants.Packaging.MacrosNodeName);
                files = rootElement.Element(Constants.Packaging.FilesNodeName);
                templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
                documentTypes = rootElement.Element(Constants.Packaging.DocumentTypesNodeName);
                styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
                documentSet = rootElement.Element(Constants.Packaging.DocumentSetNodeName);
                actions = rootElement.Element(Constants.Packaging.ActionsNodeName);

                metaData = GetMetaData(rootElement);
                installationSummary = new InstallationSummary() { MetaData = metaData };
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFile, e);
            }

            try
            {
                var dataTypeDefinitions = EmptyArrayIfNull<IDataTypeDefinition>(dataTypes) ?? InstallDataTypes(dataTypes, userId);
                installationSummary.DataTypesInstalled = dataTypeDefinitions;

                var languagesInstalled = EmptyArrayIfNull<ILanguage>(languages) ?? InstallLanguages(languages, userId);
                installationSummary.LanguagesInstalled = languagesInstalled;

                var dictionaryInstalled = EmptyArrayIfNull<IDictionaryItem>(dictionaryItems) ?? InstallDictionaryItems(dictionaryItems);
                installationSummary.DictionaryItemsInstalled = dictionaryInstalled;

                var macros = EmptyArrayIfNull<IMacro>(macroes)?? InstallMacros(macroes, userId);
                installationSummary.MacrosInstalled = macros;

                var keyValuePairs = EmptyArrayIfNull<Details<string>>(packageFile) ?? InstallFiles(packageFile, files);
                installationSummary.FilesInstalled = keyValuePairs;
                
                var templatesInstalled = EmptyArrayIfNull<ITemplate>(templates) ?? InstallTemplats(templates, userId);
                installationSummary.TemplatesInstalled = templatesInstalled;

                var documentTypesInstalled = EmptyArrayIfNull<IContentType>(documentTypes) ?? InstallDocumentTypes(documentTypes, userId);
                installationSummary.DocumentTypesInstalled =documentTypesInstalled;

                var stylesheetsInstalled = EmptyArrayIfNull<IStylesheet>(styleSheets) ?? InstallStylesheets(styleSheets, userId);
                installationSummary.StylesheetsInstalled = stylesheetsInstalled;

                var documentsInstalled = EmptyArrayIfNull<IContent>(documentSet) ?? InstallDocuments(documentSet, userId);
                installationSummary.DocumentsInstalled = documentsInstalled;

                var packageActions = EmptyArrayIfNull<PackageAction>(actions) ?? GetPackageActions(actions, metaData.Name);
                installationSummary.Actions = packageActions;

                return installationSummary;
            }
            catch (Exception e)
            {
                throw new Exception("Error installing package " + packageFile, e);
            }
        }

        /// <summary>
        /// Temperary check to test that we support stylesheets
        /// </summary>
        /// <param name="rootElement"></param>
        private void PackageSupportedCheck(XElement rootElement)
        {
            XElement styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
            if (styleSheets != null && styleSheets.Elements().Any())
                throw new NotSupportedException("Stylesheets is not suported in this version of umbraco");

        }

        private static T[] EmptyArrayIfNull<T>(object obj)
        {
            return obj == null ? new T[0] : null;
        }


        private XDocument GetConfigXmlDoc(string packageFilePath)
        {
            string filePathInPackage;
            string configXmlContent = _packageExtraction.ReadTextFileFromArchive(packageFilePath,
                Constants.Packaging.PackageXmlFileName, out filePathInPackage);

            return XDocument.Parse(configXmlContent);
        }


        private XElement GetConfigXmlElement(string packageFilePath)
        {
            XDocument document = GetConfigXmlDoc(packageFilePath);
            if (document.Root == null ||
                document.Root.Name.LocalName.Equals(Constants.Packaging.UmbPackageNodeName) == false)
            {
                throw new ArgumentException("xml does not have a root node called \"umbPackage\"", packageFilePath);
            }
            return document.Root;
        }


        internal void PackageStructureSanetyCheck(string packageFilePath)
        {
            XElement rootElement = GetConfigXmlElement(packageFilePath);
            PackageStructureSanetyCheck(packageFilePath, rootElement);
        }

        private void PackageStructureSanetyCheck(string packageFilePath, XElement rootElement)
        {
            
            XElement filesElement = rootElement.Element(Constants.Packaging.FilesNodeName);
            if (filesElement != null)
            {
                IEnumerable<FileInPackageInfo> extractFileInPackageInfos =
                    ExtractFileInPackageInfos(filesElement).ToArray();

                IEnumerable<string> missingFiles =
                    _packageExtraction.FindMissingFiles(packageFilePath,
                        extractFileInPackageInfos.Select(i => i.FileNameInPackage)).ToArray();

                if (missingFiles.Any())
                {
                    throw new Exception("The following file(s) are missing in the package: " +
                                        string.Join(", ", missingFiles.Select(
                                            mf =>
                                            {
                                                FileInPackageInfo fileInPackageInfo =
                                                    extractFileInPackageInfos.Single(fi => fi.FileNameInPackage == mf);
                                                return string.Format("Guid: \"{0}\" Original File: \"{1}\"",
                                                    fileInPackageInfo.FileNameInPackage, fileInPackageInfo.RelativePath);
                                            })));
                }

                IEnumerable<string> dubletFileNames = _packageExtraction.FindDubletFileNames(packageFilePath).ToArray();
                if (dubletFileNames.Any())
                {
                    throw new Exception("The following filename(s) are found more than one time in the package, since the filename is used ad primary key, this is not allowed: " +
                                        string.Join(", ", dubletFileNames));
                }
            }
        }




        private static PackageAction[] GetPackageActions(XElement actionsElement, string packageName)
        {
            if (actionsElement == null) { return new PackageAction[0]; }

            if (string.Equals(Constants.Packaging.ActionsNodeName, actionsElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.ActionsNodeName + "\" as root",
                    "actionsElement");
            }

            return actionsElement.Elements(Constants.Packaging.ActionNodeName)
                .Select(elemet =>
                {
                    XAttribute aliasAttr = elemet.Attribute(Constants.Packaging.AliasNodeNameCapital);
                    if (aliasAttr == null)
                        throw new ArgumentException(
                            "missing \"" + Constants.Packaging.AliasNodeNameCapital + "\" atribute in alias element",
                            "actionsElement");

                    var packageAction = new PackageAction
                    {
                        XmlData = elemet, 
                        Alias = aliasAttr.Value,
                        PackageName = packageName,
                    };


                    XAttribute attr = elemet.Attribute(Constants.Packaging.RunatNodeAttribute);

                    ActionRunAt runAt;
                    if (attr != null && Enum.TryParse(attr.Value, true, out runAt)) { packageAction.RunAt = runAt; }

                    attr = elemet.Attribute(Constants.Packaging.UndoNodeAttribute);

                    bool undo;
                    if (attr != null && bool.TryParse(attr.Value, out undo)) { packageAction.Undo = undo; }


                    return packageAction;
                }).ToArray();
        }

        private IContent[] InstallDocuments(XElement documentsElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DocumentSetNodeName, documentsElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.DocumentSetNodeName + "\" as root",
                    "documentsElement");
            }
            return _packagingService.ImportContent(documentsElement, -1, userId).ToArray();
        }

        private IStylesheet[] InstallStylesheets(XElement styleSheetsElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.StylesheetsNodeName, styleSheetsElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.StylesheetsNodeName + "\" as root",
                    "styleSheetsElement");
            }
            return _packagingService.ImportStylesheets(styleSheetsElement, userId).ToArray();
        }

        private IContentType[] InstallDocumentTypes(XElement documentTypes, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DocumentTypesNodeName, documentTypes.Name.LocalName) == false)
            {
                if (string.Equals(Constants.Packaging.DocumentTypeNodeName, documentTypes.Name.LocalName) == false)
                    throw new ArgumentException(
                        "Must be \"" + Constants.Packaging.DocumentTypesNodeName + "\" as root", "documentTypes");

                documentTypes = new XElement(Constants.Packaging.DocumentTypesNodeName, documentTypes);
            }

            return _packagingService.ImportContentTypes(documentTypes, userId).ToArray();
        }

        private ITemplate[] InstallTemplats(XElement templateElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.TemplatesNodeName, templateElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.TemplatesNodeName + "\" as root",
                    "templateElement");
            }
            return _packagingService.ImportTemplates(templateElement, userId).ToArray();
        }


        private Details<string>[] InstallFiles(string packageFilePath, XElement filesElement)
        {
            return ExtractFileInPackageInfos(filesElement).Select(fpi =>
            {
                bool existingOverrided = _packageExtraction.CopyFileFromArchive(packageFilePath, fpi.FileNameInPackage,
                    fpi.FullPath);

                return new Details<string>()
                {
                    Source = fpi.FileNameInPackage,
                    Destination = fpi.FullPath,
                    Status = existingOverrided ? InstallStatus.Overwridden : InstallStatus.Inserted
                };

            }).ToArray();
        }

        private IMacro[] InstallMacros(XElement macroElements, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.MacrosNodeName, macroElements.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.MacrosNodeName + "\" as root",
                    "macroElements");
            }
            return _packagingService.ImportMacros(macroElements, userId).ToArray();
        }

        private IDictionaryItem[] InstallDictionaryItems(XElement dictionaryItemsElement)
        {
            if (string.Equals(Constants.Packaging.DictionaryItemsNodeName, dictionaryItemsElement.Name.LocalName) ==
                false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.DictionaryItemsNodeName + "\" as root",
                    "dictionaryItemsElement");
            }
            return _packagingService.ImportDictionaryItems(dictionaryItemsElement).ToArray();
        }

        private ILanguage[] InstallLanguages(XElement languageElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.LanguagesNodeName, languageElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.LanguagesNodeName + "\" as root", "languageElement");
            }
            return _packagingService.ImportLanguages(languageElement, userId).ToArray();
        }

        private IDataTypeDefinition[] InstallDataTypes(XElement dataTypeElements, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DataTypesNodeName, dataTypeElements.Name.LocalName) == false)
            {
                if (string.Equals(Constants.Packaging.DataTypeNodeName, dataTypeElements.Name.LocalName) == false)
                {
                    throw new ArgumentException("Must be \"" + Constants.Packaging.DataTypeNodeName + "\" as root", "dataTypeElements");
                }
            }
            return _packagingService.ImportDataTypeDefinitions(dataTypeElements, userId).ToArray();
        }

        private PreInstallWarnings GetPreInstallWarnings(XElement rootElement)
        {
            XElement files = rootElement.Element(Constants.Packaging.FilesNodeName);
            XElement styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
            XElement templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
            XElement alias = rootElement.Element(Constants.Packaging.MacrosNodeName);
            var conflictingPackageContent = new PreInstallWarnings
            {
                UnsecureFiles = files == null ? new IFileInPackageInfo[0] : FindUnsecureFiles(files),
                ConflictingMacroAliases = alias == null ? new IMacro[0] : ConflictingPackageContentFinder.FindConflictingMacros(alias),
                ConflictingTemplateAliases =
                    templates == null ? new ITemplate[0] : ConflictingPackageContentFinder.FindConflictingTemplates(templates),
                ConflictingStylesheetNames =
                    styleSheets == null ? new IStylesheet[0] : ConflictingPackageContentFinder.FindConflictingStylesheets(styleSheets)
            };

            return conflictingPackageContent;
        }

        private IFileInPackageInfo[] FindUnsecureFiles(XElement fileElement)
        {
            return ExtractFileInPackageInfos(fileElement)
                .Where(IsFileNodeUnsecure).Cast<IFileInPackageInfo>().ToArray();
        }

        private bool IsFileNodeUnsecure(FileInPackageInfo fileInPackageInfo)
        {

            // Should be done with regex :)
            if (fileInPackageInfo.Directory.ToLower().Contains(IOHelper.DirSepChar + "app_code")) return true;
            if (fileInPackageInfo.Directory.ToLower().Contains(IOHelper.DirSepChar + "bin")) return true;

            string extension = Path.GetExtension(fileInPackageInfo.Directory);

            return extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase);
        }


        private IEnumerable<FileInPackageInfo> ExtractFileInPackageInfos(XElement filesElement)
        {
            if (string.Equals(Constants.Packaging.FilesNodeName, filesElement.Name.LocalName) == false)
            {
                throw new ArgumentException("the root element must be \"Files\"", "filesElement");
            }

            return filesElement.Elements(Constants.Packaging.FileNodeName)
                .Select(e =>
                {
                    XElement guidElement = e.Element(Constants.Packaging.GuidNodeName);
                    if (guidElement == null)
                    {
                        throw new ArgumentException("Missing element \"" + Constants.Packaging.GuidNodeName + "\"",
                            "filesElement");
                    }

                    XElement orgPathElement = e.Element(Constants.Packaging.OrgPathNodeName);
                    if (orgPathElement == null)
                    {
                        throw new ArgumentException("Missing element \"" + Constants.Packaging.OrgPathNodeName + "\"",
                            "filesElement");
                    }

                    XElement orgNameElement = e.Element(Constants.Packaging.OrgNameNodeName);
                    if (orgNameElement == null)
                    {
                        throw new ArgumentException("Missing element \"" + Constants.Packaging.OrgNameNodeName + "\"",
                            "filesElement");
                    }


                    return new FileInPackageInfo
                    {
                        FileNameInPackage = guidElement.Value,
                        FileName = PrepareAsFilePathElement(orgNameElement.Value),
                        RelativeDir = UpdatePathPlaceholders(
                            PrepareAsFilePathElement(orgPathElement.Value)),
                        DestinationRootDir = FullpathToRoot
                    };
                }).ToArray();
        }

        private static string PrepareAsFilePathElement(string pathElement)
        {
            return pathElement.TrimStart(new[] {'\\', '/', '~'}).Replace("/", "\\");
        }


        private MetaData GetMetaData(XElement xRootElement)
        {
            XElement infoElement = xRootElement.Element(Constants.Packaging.InfoNodeName);

            if (infoElement == null)
            {
                throw new ArgumentException("Did not hold a \"" + Constants.Packaging.InfoNodeName + "\" element",
                    "xRootElement");
            }

            var majorElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsMajorXpath);
            var minorElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsMinorXpath);
            var patchElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsPatchXpath);
            var nameElement = infoElement.XPathSelectElement(Constants.Packaging.PackageNameXpath);
            var versionElement = infoElement.XPathSelectElement(Constants.Packaging.PackageVersionXpath);
            var urlElement = infoElement.XPathSelectElement(Constants.Packaging.PackageUrlXpath);
            var licenseElement = infoElement.XPathSelectElement(Constants.Packaging.PackageLicenseXpath);
            var authorNameElement = infoElement.XPathSelectElement(Constants.Packaging.AuthorNameXpath);
            var authorUrlElement = infoElement.XPathSelectElement(Constants.Packaging.AuthorWebsiteXpath);
            var readmeElement = infoElement.XPathSelectElement(Constants.Packaging.ReadmeXpath);

            XElement controlElement = xRootElement.Element(Constants.Packaging.ControlNodeName);

            return new MetaData
            {
                Name = StringValue(nameElement),
                Version = StringValue(versionElement),
                Url = StringValue(urlElement),
                License = StringValue(licenseElement),
                LicenseUrl = StringAttribute(licenseElement, Constants.Packaging.PackageLicenseXpathUrlAttribute),
                AuthorName = StringValue(authorNameElement),
                AuthorUrl = StringValue(authorUrlElement),
                Readme = StringValue(readmeElement),
                Control = StringValue(controlElement),
                ReqMajor = IntValue(majorElement),
                ReqMinor = IntValue(minorElement),
                ReqPatch = IntValue(patchElement),
                
            };
        }

        private static string StringValue(XElement xElement, string defaultValue = "")
        {
            return xElement == null ? defaultValue : xElement.Value;
        }

        private static string StringAttribute(XElement xElement, string attribute, string defaultValue = "")
        {
            return xElement == null
                        ? defaultValue
                        : xElement.HasAttributes ? xElement.AttributeValue<string>(attribute) : defaultValue;
        }


        private static int IntValue(XElement xElement, int defaultValue = 0)
        {
            int val;
            return xElement == null ? defaultValue : int.TryParse(xElement.Value, out val) ? val : defaultValue;
        }


        private static string UpdatePathPlaceholders(string path)
        {
            if (path.Contains("[$"))
            {
                //this is experimental and undocumented...
                path = path.Replace("[$UMBRACO]", SystemDirectories.Umbraco);
                path = path.Replace("[$UMBRACOCLIENT]", SystemDirectories.UmbracoClient);
                path = path.Replace("[$CONFIG]", SystemDirectories.Config);
                path = path.Replace("[$DATA]", SystemDirectories.Data);
            }
            return path;
        }
    }

    public class FileInPackageInfo : IFileInPackageInfo
    {
        public string RelativePath
        {
            get { return Path.Combine(RelativeDir, FileName); }
        }

        public string FileNameInPackage { get; set; }
        public string RelativeDir { get; set; }
        public string DestinationRootDir { private get; set; }

        public string Directory
        {
            get { return Path.Combine(DestinationRootDir, RelativeDir); }
        }

        public string FullPath
        {
            get { return Path.Combine(DestinationRootDir, RelativePath); }
        }

        public string FileName { get; set; }
    }

    public interface IFileInPackageInfo
    {
        string RelativeDir { get; }
        string RelativePath { get; }
        string FileName { get; set; }
    }
    
}
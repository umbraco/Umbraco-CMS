using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Packaging;
using File = System.IO.File;

namespace Umbraco.Core.Services
{
    public class PackageInstallerService : IPackageInstallerService
    {
        #region consts
        private const string UMBPACKAGE_NODENAME = "umbPackage";
        private const string DATA_TYPES_NODENAME = "DataTypes";
        private const string PACKAGE_XML_FILE_NAME = "package.xml";
        private const string UMBRACO_PACKAGE_EXTENTION = ".umb";
        private const string DATA_TYPE_NODENAME = "DataType";
        private const string LANGUAGES_NODENAME = "Languages";
        private const string FILES_NODENAME = "Files";
        private const string STYLESHEETS_NODENAME = "Stylesheets";
        private const string TEMPLATES_NODENAME = "Templates";
        private const string ORGNAME_NODENAME = "orgName";
        private const string NAME_NODENAME = "Name";
        private const string TEMPLATE_NODENAME = "Template";
        private const string ALIAS_NODENAME = "Alias";
        private const string DICTIONARYITEMS_NODENAME = "DictionaryItems";
        private const string MACROS_NODENAME = "macros";
        private const string DOCUMENTSET_NODENAME = "DocumentSet";
        private const string DOCUMENTTYPES_NODENAME = "DocumentTypes";
        private const string DOCUMENTTYPE_NODENAME = "DocumentType";
        private const string FILE_NODENAME = "file";
        private const string ORGPATH_NODENAME = "orgPath";
        private const string GUID_NODENAME = "guid";
        private const string STYLESHEET_NODENAME = "styleSheet";
        private const string MACRO_NODENAME = "macro";
        private const string INFO_NODENAME = "info";
        private const string PACKAGE_REQUIREMENTS_MAJOR_XPATH = "/package/requirements/major";
        private const string PACKAGE_REQUIREMENTS_MINOR_XPATH = "/package/requirements/minor";
        private const string PACKAGE_REQUIREMENTS_PATCH_XPATH = "/package/requirements/patch";
        private const string PACKAGE_NAME_XPATH = "/package/name";
        private const string PACKAGE_VERSION_XPATH = "/package/version";
        private const string PACKAGE_URL_XPATH = "/package/url";
        private const string PACKAGE_LICENSE_XPATH = "/package/license";
        private const string AUTHOR_NAME_XPATH = "/author/name";
        private const string AUTHOR_WEBSITE_XPATH = "/author/website";
        private const string README_XPATH = "/readme";
        private const string CONTROL_NODENAME = "control";
        private const string ACTION_NODENAME = "Action";
        private const string ACTIONS_NODENAME = "Actions";
        private const string UNDO_NODEATTRIBUTE = "undo";
        private const string RUNAT_NODEATTRIBUTE = "runat";

        #endregion

        private readonly IPackageValidationHelper _packageValidationHelper;
        private readonly IPackagingService _packagingService;
        private readonly IUnpackHelper _unpackHelper;


        public PackageInstallerService(IPackagingService packagingService, IUnpackHelper unpackHelper, IPackageValidationHelper packageValidationHelper)
        {
            if (packageValidationHelper != null) _packageValidationHelper = packageValidationHelper; else throw new ArgumentNullException("packageValidationHelper");
            if (packagingService != null) _packagingService = packagingService; else throw new ArgumentNullException("packagingService");
            if (unpackHelper != null) _unpackHelper = unpackHelper; else throw new ArgumentNullException("unpackHelper");
        }


        public PackageInstallationSummary InstallPackageFile(string packageFilePath, int userId)
        {
            FileInfo fi = GetPackageFileInfo(packageFilePath);
            string tempDir = null;
            try
            {
                tempDir = _unpackHelper.UnPackToTempDirectory(fi.FullName);
                return InstallFromDirectory(tempDir, userId);
            }
            finally
            {
                if (string.IsNullOrEmpty(tempDir) == false && Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        public PackageMetaData GetMetaData(string packageFilePath)
        {
            var rootElement = GetConfigXmlRootElementFromPackageFile(packageFilePath);
            return GetMetaData(rootElement);
        }

        public PackageImportIssues FindPackageImportIssues(string packageFilePath)
        {
            var rootElement = GetConfigXmlRootElementFromPackageFile(packageFilePath);
            return FindImportIssues(rootElement);
        }


        private FileInfo GetPackageFileInfo(string packageFilePath)
        {
            if (string.IsNullOrEmpty(packageFilePath))
            {
                throw new ArgumentNullException("packageFilePath");
            }

            var fi = new FileInfo(packageFilePath);

            if (fi.Exists == false)
            {
                throw new Exception("Error - file not found. Could find file named '" + packageFilePath + "'");
            }


            // Check if the file is a valid package
            if (fi.Extension.Equals(UMBRACO_PACKAGE_EXTENTION, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception("Error - file isn't a package (doesn't have a .umb extension). Check if the file automatically got named '.zip' upon download.");
            }

            return fi;
        }


        private XDocument GetConfigXmlDocFromPackageFile(string packageFilePath)
        {
            FileInfo packageFileInfo = GetPackageFileInfo(packageFilePath);

            string configXmlContent = _unpackHelper.ReadTextFileFromArchive(packageFileInfo.FullName, PACKAGE_XML_FILE_NAME);

            return XDocument.Parse(configXmlContent);
        }


        private XElement GetConfigXmlRootElementFromPackageFile(string packageFilePath)
        {
            var document = GetConfigXmlDocFromPackageFile(packageFilePath);
            if (document.Root == null || document.Root.Name.LocalName.Equals(UMBPACKAGE_NODENAME) == false) { throw new ArgumentException("xml does not have a root node called \"umbPackage\"", packageFilePath); }
            return document.Root;
        }


        private PackageInstallationSummary InstallFromDirectory(string packageDir, int userId)
        {
            var configXml = GetConfigXmlDocFromPackageDirectory(packageDir);
            var rootElement = configXml.XPathSelectElement(UMBPACKAGE_NODENAME);
            if (rootElement == null) { throw new ArgumentException("File does not have a root node called \"" + UMBPACKAGE_NODENAME + "\"", packageDir); }

            var dataTypes = rootElement.Element(DATA_TYPES_NODENAME);
            var languages = rootElement.Element(LANGUAGES_NODENAME);
            var dictionaryItems = rootElement.Element(DICTIONARYITEMS_NODENAME);
            var macroes = rootElement.Element(MACROS_NODENAME);
            var files = rootElement.Element(FILES_NODENAME);
            var templates = rootElement.Element(TEMPLATES_NODENAME);
            var documentTypes = rootElement.Element(DOCUMENTTYPES_NODENAME);
            var styleSheets = rootElement.Element(STYLESHEETS_NODENAME);
            var documentSet = rootElement.Element(DOCUMENTSET_NODENAME);
            var actions = rootElement.Element(ACTIONS_NODENAME);

            return new PackageInstallationSummary
            {
                MetaData = GetMetaData(rootElement),
                DataTypesInstalled = dataTypes == null ? Enumerable.Empty<int>() : InstallDataTypes(dataTypes, userId),
                LanguagesInstalled = languages == null ? Enumerable.Empty<int>() : InstallLanguages(languages, userId),
                DictionaryItemsInstalled = dictionaryItems == null ? Enumerable.Empty<int>() : InstallDictionaryItems(dictionaryItems, userId),
                MacrosInstalled = macroes == null ? Enumerable.Empty<int>() : InstallMacros(macroes, userId),
                FilesInstalled = packageDir == null ? Enumerable.Empty<KeyValuePair<string, bool>>() : InstallFiles(packageDir, files),
                TemplatesInstalled = templates == null ? Enumerable.Empty<int>() : InstallTemplats(templates, userId),
                DocumentTypesInstalled = documentTypes == null ? Enumerable.Empty<int>() : InstallDocumentTypes(documentTypes, userId),
                StylesheetsInstalled = styleSheets == null ? Enumerable.Empty<int>() : InstallStylesheets(styleSheets, userId),
                DocumentsInstalled = documentSet == null ? Enumerable.Empty<int>() : InstallDocuments(documentSet, userId),
                PackageInstallActions = actions == null ? Enumerable.Empty<KeyValuePair<string, XElement>>() : GetInstallActions(actions),
                PackageUninstallActions = actions == null ? string.Empty : GetUninstallActions(actions)
            };
        }

        private static string GetUninstallActions(XElement actionsElement)
        {
            //saving the uninstall actions untill the package is uninstalled.
            return actionsElement.Elements(ACTION_NODENAME).Where(e => e.HasAttributes && e.Attribute(UNDO_NODEATTRIBUTE) != null && e.Attribute(UNDO_NODEATTRIBUTE).Value.Equals("false()", StringComparison.InvariantCultureIgnoreCase) == false) // SelectNodes("Actions/Action [@undo != false()]");
                .Select(m => m.Value).Aggregate((workingSentence, next) => next + workingSentence);
        }

        private static IEnumerable<KeyValuePair<string, XElement>> GetInstallActions(XElement actionsElement)
        {
            if (actionsElement == null) { return Enumerable.Empty<KeyValuePair<string, XElement>>(); }

            if (string.Equals(ACTIONS_NODENAME, actionsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"" + ACTIONS_NODENAME + "\" as root", "actionsElement"); }

            return actionsElement.Elements(ACTION_NODENAME)
                .Where(
                    e =>
                        e.HasAttributes &&
                        (e.Attribute(RUNAT_NODEATTRIBUTE) == null ||
                         e.Attribute(RUNAT_NODEATTRIBUTE).Value.Equals("uninstall", StringComparison.InvariantCultureIgnoreCase) ==
                         false)) // .SelectNodes("Actions/Action [@runat != 'uninstall']")
                .Select(elemet =>
                {
                    var aliasAttr = elemet.Attribute(ALIAS_NODENAME);
                    if (aliasAttr == null)
                        throw new ArgumentException("missing \"" + ALIAS_NODENAME + "\" atribute in alias element", "actionsElement");
                    return new {elemet, alias = aliasAttr.Value};
                }).ToDictionary(x => x.alias, x => x.elemet);
        }

        private IEnumerable<int> InstallDocuments(XElement documentsElement, int userId = 0)
        {
            if (string.Equals(DOCUMENTSET_NODENAME, documentsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"DocumentSet\" as root", "documentsElement"); }
            return _packagingService.ImportContent(documentsElement, -1, userId).Select(c => c.Id);
        }

        private IEnumerable<int> InstallStylesheets(XElement styleSheetsElement, int userId = 0)
        {
            if (string.Equals(STYLESHEETS_NODENAME, styleSheetsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Stylesheets\" as root", "styleSheetsElement"); }
            return _packagingService.ImportStylesheets(styleSheetsElement, userId).Select(f => f.Id);
        }

        private IEnumerable<int> InstallDocumentTypes(XElement documentTypes, int userId = 0)
        {
            if (string.Equals(DOCUMENTTYPES_NODENAME, documentTypes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                if (string.Equals(DOCUMENTTYPE_NODENAME, documentTypes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
                    throw new ArgumentException("Must be \"" + DOCUMENTTYPES_NODENAME + "\" as root", "documentTypes");

                documentTypes = new XElement(DOCUMENTTYPES_NODENAME, documentTypes);
            }

            return _packagingService.ImportContentTypes(documentTypes, userId).Select(ct => ct.Id);
        }

        private IEnumerable<int> InstallTemplats(XElement templateElement, int userId = 0)
        {
            if (string.Equals(TEMPLATES_NODENAME, templateElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"" + TEMPLATES_NODENAME + "\" as root", "templateElement"); }
            return _packagingService.ImportTemplates(templateElement, userId).Select(t => t.Id);
        }


        private static IEnumerable<KeyValuePair<string, bool>> InstallFiles(string packageDir, XElement filesElement)
        {
            if (string.Equals(FILES_NODENAME, filesElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("root element must be \"" + FILES_NODENAME + "\"", "filesElement"); }

            string basePath = HostingEnvironment.ApplicationPhysicalPath;
            
            var xmlNodeList = filesElement.Elements(FILE_NODENAME);

            return xmlNodeList.Select(e =>
            {
                var orgPathElement = e.Element(ORGPATH_NODENAME);
                if (orgPathElement == null) { throw new ArgumentException("Missing element \"" + ORGPATH_NODENAME + "\"", "filesElement"); }

                var guidElement = e.Element(GUID_NODENAME);
                if (guidElement == null) { throw new ArgumentException("Missing element \"" + GUID_NODENAME + "\"", "filesElement"); }

                var orgNameElement = e.Element(ORGNAME_NODENAME);
                if (orgNameElement == null) { throw new ArgumentException("Missing element \"" + ORGNAME_NODENAME + "\"", "filesElement"); }


                var destPath = GetFileName(basePath, orgPathElement.Value);
                var sourceFile = GetFileName(packageDir, guidElement.Value);
                var destFile = GetFileName(destPath, orgNameElement.Value);

                if (Directory.Exists(destPath) == false) Directory.CreateDirectory(destPath);

                var existingOverrided = File.Exists(destFile);

                File.Copy(sourceFile, destFile, true);
                
                return new KeyValuePair<string, bool>(orgPathElement.Value + "/" + orgNameElement.Value, existingOverrided);
            });
        }

        private IEnumerable<int> InstallMacros(XElement macroElements, int userId = 0)
        {
            if (string.Equals(MACROS_NODENAME, macroElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Templates\" as root", "macroElements"); }
            return _packagingService.ImportMacros(macroElements, userId).Select(m => m.Id);
        }

        private IEnumerable<int> InstallDictionaryItems(XElement dictionaryItemsElement, int userId = 0)
        {
            if (string.Equals(DICTIONARYITEMS_NODENAME, dictionaryItemsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Templates\" as root", "dictionaryItemsElement"); }
            return _packagingService.ImportDictionaryItems(dictionaryItemsElement, userId).Select(di => di.Id);
        }

        private IEnumerable<int> InstallLanguages(XElement languageElement, int userId = 0)
        {
            if (string.Equals(LANGUAGES_NODENAME, languageElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Templates\" as root", "languageElement"); }
            return _packagingService.ImportLanguage(languageElement, userId).Select(l => l.Id);
        }

        private IEnumerable<int> InstallDataTypes(XElement dataTypeElements, int userId = 0)
        {
            if (string.Equals(DATA_TYPES_NODENAME, dataTypeElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
            {

                if (string.Equals(DATA_TYPE_NODENAME, dataTypeElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    throw new ArgumentException("Must be \"Templates\" as root", "dataTypeElements");
                }
            }
            return _packagingService.ImportDataTypeDefinitions(dataTypeElements, userId).Select(e => e.Id);
        }

        private static XDocument GetConfigXmlDocFromPackageDirectory(string packageDir)
        {
            string packageXmlPath = Path.Combine(packageDir, PACKAGE_XML_FILE_NAME);
            if (File.Exists(packageXmlPath) == false) { throw new FileNotFoundException("Could not find " + PACKAGE_XML_FILE_NAME + " in package"); }
            return XDocument.Load(packageXmlPath);
        }


        private PackageImportIssues FindImportIssues(XElement rootElement)
        {
            var files = rootElement.Element(FILES_NODENAME);
            var styleSheets = rootElement.Element(STYLESHEETS_NODENAME);
            var templates = rootElement.Element(TEMPLATES_NODENAME);
            var alias = rootElement.Element(MACROS_NODENAME);
            var packageImportIssues = new PackageImportIssues
            {
                UnsecureFiles = files == null ? Enumerable.Empty<string>() : FindUnsecureFiles(files),
                ConflictingMacroAliases =  alias == null ? Enumerable.Empty<KeyValuePair<string, string>>() : FindConflictingMacroAliases(alias),
                ConflictingTemplateAliases = templates == null ?  Enumerable.Empty<KeyValuePair<string, string>>() : FindConflictingTemplateAliases(templates),
                ConflictingStylesheetNames = styleSheets == null ? Enumerable.Empty<KeyValuePair<string, string>>() : FindConflictingStylesheetNames(styleSheets)
            };

            return packageImportIssues;
        }

        private IEnumerable<string> FindUnsecureFiles(XElement fileElement)
        {
            if (string.Equals(FILES_NODENAME, fileElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("the root element must be \"Files\"", "fileElement"); }

            return fileElement.Elements(FILE_NODENAME)
                .Where(FileNodeIsUnsecure)
                    .Select(n =>
                    {
                        var xElement = n.Element(ORGNAME_NODENAME);
                        if (xElement == null) { throw new ArgumentException("missing a element: " + ORGNAME_NODENAME, "n"); }
                        return xElement.Value;
                    });
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingStylesheetNames(XElement stylesheetNotes)
        {
            if (string.Equals(STYLESHEETS_NODENAME, stylesheetNotes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("the root element must be \"Stylesheets\"", "stylesheetNotes"); }

            return stylesheetNotes.Elements(STYLESHEET_NODENAME)
                    .Select(n =>
                    {
                        var xElement = n.Element(NAME_NODENAME);
                        if (xElement == null) { throw new ArgumentException("Missing \"" + NAME_NODENAME + "\" element", "stylesheetNotes"); }

                        string name = xElement.Value;

                        Stylesheet existingStyleSheet;
                        if (_packageValidationHelper.StylesheetExists(name, out existingStyleSheet))
                        {
                            // Don't know what to put in here... existing path was the best i could come up with
                            return new KeyValuePair<string, string>(name, existingStyleSheet.Path);
                        }
                        return new KeyValuePair<string, string>(name, null);
                    })
                    .Where(kv => kv.Value != null);
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingTemplateAliases(XElement templateNotes)
        {
            if (string.Equals(TEMPLATES_NODENAME, templateNotes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Node must be a \"" + TEMPLATES_NODENAME + "\" node", "templateNotes"); }

            return templateNotes.Elements(TEMPLATE_NODENAME)
                    .Select(n =>
                    {
                        var alias = n.Element(ALIAS_NODENAME);
                        if (alias == null) { throw new ArgumentException("missing a \"" + ALIAS_NODENAME + "\" element", "templateNotes"); }
                        string aliasStr = alias.Value;

                        ITemplate existingTemplate;

                        if (_packageValidationHelper.TemplateExists(aliasStr, out existingTemplate))
                        {
                            return new KeyValuePair<string, string>(aliasStr, existingTemplate.Name);
                        }

                        return new KeyValuePair<string, string>(aliasStr, null);
                    })
                    .Where(kv => kv.Value != null);
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingMacroAliases(XElement macroNodes)
        {
            return  macroNodes.Elements(MACRO_NODENAME)
                    .Select(n =>
                    {
                        var xElement = n.Element(ALIAS_NODENAME);
                        if (xElement == null) { throw new ArgumentException("missing a \"" + ALIAS_NODENAME + "\" element", "macroNodes"); }
                        string alias = xElement.Value;

                        IMacro existingMacro;
                        if (_packageValidationHelper.MacroExists(alias, out existingMacro))
                        {
                            return new KeyValuePair<string, string>(alias, existingMacro.Name);
                        }
                        
                        return new KeyValuePair<string, string>(alias, null);
                    })
                    .Where(kv => kv.Key != null && kv.Value != null);
        }


        private bool FileNodeIsUnsecure(XElement fileNode)
        {
            string basePath = HostingEnvironment.ApplicationPhysicalPath;
            var orgName = fileNode.Element(ORGNAME_NODENAME);
            if (orgName == null) { throw new ArgumentException("Missing element \"" + ORGNAME_NODENAME + "\"", "fileNode"); }

            string destPath = GetFileName(basePath, orgName.Value);

            // Should be done with regex :)
            if (destPath.ToLower().Contains(IOHelper.DirSepChar + "app_code")) return true;
            if (destPath.ToLower().Contains(IOHelper.DirSepChar + "bin")) return true;

            return destPath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase);
        }


        private PackageMetaData GetMetaData(XElement xRootElement)
        {
            XElement infoElement = xRootElement.Element(INFO_NODENAME);

            if (infoElement == null) { throw new ArgumentException("Did not hold a \"" + INFO_NODENAME + "\" element", "xRootElement"); }

            var majorElement = infoElement.XPathSelectElement(PACKAGE_REQUIREMENTS_MAJOR_XPATH);
            var minorElement = infoElement.XPathSelectElement(PACKAGE_REQUIREMENTS_MINOR_XPATH);
            var patchElement = infoElement.XPathSelectElement(PACKAGE_REQUIREMENTS_PATCH_XPATH);
            var nameElement = infoElement.XPathSelectElement(PACKAGE_NAME_XPATH);
            var versionElement = infoElement.XPathSelectElement(PACKAGE_VERSION_XPATH);
            var urlElement = infoElement.XPathSelectElement(PACKAGE_URL_XPATH);
            var licenseElement = infoElement.XPathSelectElement(PACKAGE_LICENSE_XPATH);
            var authorNameElement = infoElement.XPathSelectElement(AUTHOR_NAME_XPATH);
            var authorUrlElement = infoElement.XPathSelectElement(AUTHOR_WEBSITE_XPATH);
            var readmeElement = infoElement.XPathSelectElement(README_XPATH);

            var controlElement = xRootElement.Element(CONTROL_NODENAME);

            int val;

            return new PackageMetaData
            {
                Name = nameElement == null ? string.Empty : nameElement.Value,
                Version = versionElement == null ? string.Empty : versionElement.Value,
                Url = urlElement == null ? string.Empty : urlElement.Value,
                License = licenseElement == null ? string.Empty : licenseElement.Value,
                LicenseUrl = licenseElement == null ? string.Empty : licenseElement.HasAttributes ? licenseElement.AttributeValue<string>("url") : string.Empty,
                AuthorName = authorNameElement == null ? string.Empty : authorNameElement.Value,
                AuthorUrl = authorUrlElement == null ? string.Empty : authorUrlElement.Value,
                Readme = readmeElement == null ? string.Empty : readmeElement.Value,
                ReqMajor = majorElement == null ? 0 : int.TryParse(majorElement.Value, out val) ? val : 0,
                ReqMinor = minorElement == null ? 0 : int.TryParse(minorElement.Value, out val) ? val : 0,
                ReqPatch = patchElement == null ? 0 : int.TryParse(patchElement.Value, out val) ? val : 0,
                Control = controlElement == null ? string.Empty : controlElement.Value
            };
        }


        /// <summary>
        ///     Gets the name of the file in the specified path.
        ///     Corrects possible problems with slashes that would result from a simple concatenation.
        ///     Can also be used to concatenate paths.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The name of the file in the specified path.</returns>
        private static String GetFileName(string path, string fileName)
        {
            // virtual dir support
            fileName = IOHelper.FindFile(fileName);

            if (path.Contains("[$"))
            {
                //this is experimental and undocumented...
                path = path.Replace("[$UMBRACO]", SystemDirectories.Umbraco);
                path = path.Replace("[$UMBRACOCLIENT]", SystemDirectories.UmbracoClient);
                path = path.Replace("[$CONFIG]", SystemDirectories.Config);
                path = path.Replace("[$DATA]", SystemDirectories.Data);
            }

            //to support virtual dirs we try to lookup the file... 
            path = IOHelper.FindFile(path);

            Debug.Assert(path != null && path.Length >= 1);
            Debug.Assert(fileName != null && fileName.Length >= 1);

            path = path.Replace('/', '\\');
            fileName = fileName.Replace('/', '\\');

            // Does filename start with a slash? Does path end with one?
            bool fileNameStartsWithSlash = (fileName[0] == Path.DirectorySeparatorChar);
            bool pathEndsWithSlash = (path[path.Length - 1] == Path.DirectorySeparatorChar);

            // Path ends with a slash
            if (pathEndsWithSlash)
            {
                if (fileNameStartsWithSlash == false)
                    // No double slash, just concatenate
                    return path + fileName;
                return path + fileName.Substring(1);
            }
            if (fileNameStartsWithSlash)
                // Required slash specified, just concatenate
                return path + fileName;
            return path + Path.DirectorySeparatorChar + fileName;
        }
    }
}
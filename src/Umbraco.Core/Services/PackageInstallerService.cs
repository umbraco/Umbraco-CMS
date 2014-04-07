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
            if (fi.Extension.Equals(Constants.Packaging.UmbracoPackageExtention, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception("Error - file isn't a package (doesn't have a .umb extension). Check if the file automatically got named '.zip' upon download.");
            }

            return fi;
        }


        private XDocument GetConfigXmlDocFromPackageFile(string packageFilePath)
        {
            FileInfo packageFileInfo = GetPackageFileInfo(packageFilePath);

            string configXmlContent = _unpackHelper.ReadTextFileFromArchive(packageFileInfo.FullName, Constants.Packaging.PackageXmlFileName);

            return XDocument.Parse(configXmlContent);
        }


        private XElement GetConfigXmlRootElementFromPackageFile(string packageFilePath)
        {
            var document = GetConfigXmlDocFromPackageFile(packageFilePath);
            if (document.Root == null || document.Root.Name.LocalName.Equals(Constants.Packaging.UmbPackageNodeName) == false) { throw new ArgumentException("xml does not have a root node called \"umbPackage\"", packageFilePath); }
            return document.Root;
        }


        private PackageInstallationSummary InstallFromDirectory(string packageDir, int userId)
        {
            var configXml = GetConfigXmlDocFromPackageDirectory(packageDir);
            var rootElement = configXml.XPathSelectElement(Constants.Packaging.UmbPackageNodeName);
            if (rootElement == null) { throw new ArgumentException("File does not have a root node called \"" + Constants.Packaging.UmbPackageNodeName + "\"", packageDir); }

            var dataTypes = rootElement.Element(Constants.Packaging.DataTypesNodeName);
            var languages = rootElement.Element(Constants.Packaging.LanguagesNodeName);
            var dictionaryItems = rootElement.Element(Constants.Packaging.DictionaryitemsNodeName);
            var macroes = rootElement.Element(Constants.Packaging.MacrosNodeName);
            var files = rootElement.Element(Constants.Packaging.FilesNodeName);
            var templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
            var documentTypes = rootElement.Element(Constants.Packaging.DocumentTypesNodeName);
            var styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
            var documentSet = rootElement.Element(Constants.Packaging.DocumentSetNodeName);
            var actions = rootElement.Element(Constants.Packaging.ActionsNodeName);

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
            return actionsElement.Elements(Constants.Packaging.ActionNodeName)
                .Where(e => e.HasAttributes && e.Attribute(Constants.Packaging.UndoNodeAttribute) != null && e.Attribute(Constants.Packaging.UndoNodeAttribute)
                    .Value.Equals("false()", StringComparison.InvariantCultureIgnoreCase) == false)  // SelectNodes("Actions/Action [@undo != false()]");
                    .Select(m => m.Value).Aggregate((workingSentence, next) => next + workingSentence);
        }

        private static IEnumerable<KeyValuePair<string, XElement>> GetInstallActions(XElement actionsElement)
        {
            if (actionsElement == null) { return Enumerable.Empty<KeyValuePair<string, XElement>>(); }

            if (string.Equals(Constants.Packaging.ActionsNodeName, actionsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"" + Constants.Packaging.ActionsNodeName + "\" as root", "actionsElement"); }

            return actionsElement.Elements(Constants.Packaging.ActionNodeName)
                .Where(
                    e =>
                        e.HasAttributes &&
                        (e.Attribute(Constants.Packaging.RunatNodeAttribute) == null ||
                         e.Attribute(Constants.Packaging.RunatNodeAttribute).Value.Equals("uninstall", StringComparison.InvariantCultureIgnoreCase) ==
                         false)) // .SelectNodes("Actions/Action [@runat != 'uninstall']")
                .Select(elemet =>
                {
                    var aliasAttr = elemet.Attribute(Constants.Packaging.AliasNodeName);
                    if (aliasAttr == null)
                        throw new ArgumentException("missing \"" + Constants.Packaging.AliasNodeName + "\" atribute in alias element", "actionsElement");
                    return new {elemet, alias = aliasAttr.Value};
                }).ToDictionary(x => x.alias, x => x.elemet);
        }

        private IEnumerable<int> InstallDocuments(XElement documentsElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DocumentSetNodeName, documentsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"" + Constants.Packaging.DocumentSetNodeName + "\" as root", "documentsElement"); }
            return _packagingService.ImportContent(documentsElement, -1, userId).Select(c => c.Id);
        }

        private IEnumerable<int> InstallStylesheets(XElement styleSheetsElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.StylesheetsNodeName, styleSheetsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"" + Constants.Packaging.StylesheetsNodeName + "\" as root", "styleSheetsElement"); }
            return _packagingService.ImportStylesheets(styleSheetsElement, userId).Select(f => f.Id);
        }

        private IEnumerable<int> InstallDocumentTypes(XElement documentTypes, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DocumentTypesNodeName, documentTypes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                if (string.Equals(Constants.Packaging.DocumentTypeNodeName, documentTypes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
                    throw new ArgumentException("Must be \"" + Constants.Packaging.DocumentTypesNodeName + "\" as root", "documentTypes");

                documentTypes = new XElement(Constants.Packaging.DocumentTypesNodeName, documentTypes);
            }

            return _packagingService.ImportContentTypes(documentTypes, userId).Select(ct => ct.Id);
        }

        private IEnumerable<int> InstallTemplats(XElement templateElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.TemplatesNodeName, templateElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"" + Constants.Packaging.TemplatesNodeName + "\" as root", "templateElement"); }
            return _packagingService.ImportTemplates(templateElement, userId).Select(t => t.Id);
        }


        private static IEnumerable<KeyValuePair<string, bool>> InstallFiles(string packageDir, XElement filesElement)
        {
            if (string.Equals(Constants.Packaging.FilesNodeName, filesElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("root element must be \"" + Constants.Packaging.FilesNodeName + "\"", "filesElement"); }

            string basePath = HostingEnvironment.ApplicationPhysicalPath;

            var xmlNodeList = filesElement.Elements(Constants.Packaging.FileNodeName);

            return xmlNodeList.Select(e =>
            {
                var orgPathElement = e.Element(Constants.Packaging.OrgPathNodeName);
                if (orgPathElement == null) { throw new ArgumentException("Missing element \"" + Constants.Packaging.OrgPathNodeName + "\"", "filesElement"); }

                var guidElement = e.Element(Constants.Packaging.GuidNodeName);
                if (guidElement == null) { throw new ArgumentException("Missing element \"" + Constants.Packaging.GuidNodeName + "\"", "filesElement"); }

                var orgNameElement = e.Element(Constants.Packaging.OrgnameNodeName);
                if (orgNameElement == null) { throw new ArgumentException("Missing element \"" + Constants.Packaging.OrgnameNodeName + "\"", "filesElement"); }


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
            if (string.Equals(Constants.Packaging.MacrosNodeName, macroElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"" + Constants.Packaging.MacrosNodeName + "\" as root", "macroElements"); }
            return _packagingService.ImportMacros(macroElements, userId).Select(m => m.Id);
        }

        private IEnumerable<int> InstallDictionaryItems(XElement dictionaryItemsElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DictionaryitemsNodeName, dictionaryItemsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"" + Constants.Packaging.DictionaryitemsNodeName + "\" as root", "dictionaryItemsElement"); }
            return _packagingService.ImportDictionaryItems(dictionaryItemsElement, userId).Select(di => di.Id);
        }

        private IEnumerable<int> InstallLanguages(XElement languageElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.LanguagesNodeName, languageElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Templates\" as root", "languageElement"); }
            return _packagingService.ImportLanguage(languageElement, userId).Select(l => l.Id);
        }

        private IEnumerable<int> InstallDataTypes(XElement dataTypeElements, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DataTypesNodeName, dataTypeElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
            {

                if (string.Equals(Constants.Packaging.DataTypeNodeName, dataTypeElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    throw new ArgumentException("Must be \"Templates\" as root", "dataTypeElements");
                }
            }
            return _packagingService.ImportDataTypeDefinitions(dataTypeElements, userId).Select(e => e.Id);
        }

        private static XDocument GetConfigXmlDocFromPackageDirectory(string packageDir)
        {
            string packageXmlPath = Path.Combine(packageDir, Constants.Packaging.PackageXmlFileName);
            if (File.Exists(packageXmlPath) == false) { throw new FileNotFoundException("Could not find " + Constants.Packaging.PackageXmlFileName + " in package"); }
            return XDocument.Load(packageXmlPath);
        }


        private PackageImportIssues FindImportIssues(XElement rootElement)
        {
            var files = rootElement.Element(Constants.Packaging.FilesNodeName);
            var styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
            var templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
            var alias = rootElement.Element(Constants.Packaging.MacrosNodeName);
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
            if (string.Equals(Constants.Packaging.FilesNodeName, fileElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("the root element must be \"Files\"", "fileElement"); }

            return fileElement.Elements(Constants.Packaging.FileNodeName)
                .Where(FileNodeIsUnsecure)
                    .Select(n =>
                    {
                        var xElement = n.Element(Constants.Packaging.OrgnameNodeName);
                        if (xElement == null) { throw new ArgumentException("missing a element: " + Constants.Packaging.OrgnameNodeName, "n"); }
                        return xElement.Value;
                    });
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingStylesheetNames(XElement stylesheetNotes)
        {
            if (string.Equals(Constants.Packaging.StylesheetsNodeName, stylesheetNotes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("the root element must be \"Stylesheets\"", "stylesheetNotes"); }

            return stylesheetNotes.Elements(Constants.Packaging.StylesheetNodeName)
                    .Select(n =>
                    {
                        var xElement = n.Element(Constants.Packaging.NameNodeName);
                        if (xElement == null) { throw new ArgumentException("Missing \"" + Constants.Packaging.NameNodeName + "\" element", "stylesheetNotes"); }

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
            if (string.Equals(Constants.Packaging.TemplatesNodeName, templateNotes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Node must be a \"" + Constants.Packaging.TemplatesNodeName + "\" node", "templateNotes"); }

            return templateNotes.Elements(Constants.Packaging.TemplateNodeName)
                    .Select(n =>
                    {
                        var alias = n.Element(Constants.Packaging.AliasNodeName);
                        if (alias == null) { throw new ArgumentException("missing a \"" + Constants.Packaging.AliasNodeName + "\" element", "templateNotes"); }
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
            return macroNodes.Elements(Constants.Packaging.MacroNodeName)
                    .Select(n =>
                    {
                        var xElement = n.Element(Constants.Packaging.AliasNodeName);
                        if (xElement == null) { throw new ArgumentException("missing a \"" + Constants.Packaging.AliasNodeName + "\" element", "macroNodes"); }
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
            var orgName = fileNode.Element(Constants.Packaging.OrgnameNodeName);
            if (orgName == null) { throw new ArgumentException("Missing element \"" + Constants.Packaging.OrgnameNodeName + "\"", "fileNode"); }

            string destPath = GetFileName(basePath, orgName.Value);

            // Should be done with regex :)
            if (destPath.ToLower().Contains(IOHelper.DirSepChar + "app_code")) return true;
            if (destPath.ToLower().Contains(IOHelper.DirSepChar + "bin")) return true;

            return destPath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase);
        }


        private PackageMetaData GetMetaData(XElement xRootElement)
        {
            XElement infoElement = xRootElement.Element(Constants.Packaging.InfoNodeName);

            if (infoElement == null) { throw new ArgumentException("Did not hold a \"" + Constants.Packaging.InfoNodeName + "\" element", "xRootElement"); }

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

            var controlElement = xRootElement.Element(Constants.Packaging.ControlNodeName);

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging
{
    /// <summary>
    /// Parses the xml document contained in a compiled (zip) Umbraco package
    /// </summary>
    public class CompiledPackageXmlParser
    {
        private readonly ConflictingPackageData _conflictingPackageData;
        private readonly GlobalSettings _globalSettings;

        public CompiledPackageXmlParser(ConflictingPackageData conflictingPackageData, IOptions<GlobalSettings> globalSettings)
        {
            _conflictingPackageData = conflictingPackageData;
            _globalSettings = globalSettings.Value;
        }

        public CompiledPackage ToCompiledPackage(XDocument xml, FileInfo packageFile, string applicationRootFolder)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException(nameof(xml), "The xml document is invalid");
            if (xml.Root.Name != "umbPackage") throw new FormatException("The xml document is invalid");

            var info = xml.Root.Element("info");
            if (info == null) throw new FormatException("The xml document is invalid");
            var package = info.Element("package");
            if (package == null) throw new FormatException("The xml document is invalid");
            var author = info.Element("author");
            if (author == null) throw new FormatException("The xml document is invalid");
            var requirements = package.Element("requirements");
            if (requirements == null) throw new FormatException("The xml document is invalid");

            var def = new CompiledPackage
            {
                PackageFile = packageFile,
                Name = package.Element("name")?.Value,
                Author = author.Element("name")?.Value,
                AuthorUrl = author.Element("website")?.Value,
                Contributors = info.Element("contributors")?.Elements("contributor").Select(x => x.Value).ToList() ?? new List<string>(),
                Version = package.Element("version")?.Value,
                Readme = info.Element("readme")?.Value,
                License = package.Element("license")?.Value,
                LicenseUrl = package.Element("license")?.AttributeValue<string>("url"),
                Url = package.Element("url")?.Value,
                IconUrl = package.Element("iconUrl")?.Value,
                UmbracoVersion = new Version((int)requirements.Element("major"), (int)requirements.Element("minor"), (int)requirements.Element("patch")),
                UmbracoVersionRequirementsType = requirements.AttributeValue<string>("type").IsNullOrWhiteSpace() ? RequirementsType.Legacy : Enum<RequirementsType>.Parse(requirements.AttributeValue<string>("type"), true),
                PackageView = xml.Root.Element("view")?.Value,
                Actions = xml.Root.Element("Actions")?.ToString(SaveOptions.None) ?? "<Actions></Actions>", //take the entire outer xml value
                Files = xml.Root.Element("files")?.Elements("file")?.Select(CompiledPackageFile.Create).ToList() ?? new List<CompiledPackageFile>(),
                Macros = xml.Root.Element("Macros")?.Elements("macro") ?? Enumerable.Empty<XElement>(),
                Templates = xml.Root.Element("Templates")?.Elements("Template") ?? Enumerable.Empty<XElement>(),
                Stylesheets = xml.Root.Element("Stylesheets")?.Elements("styleSheet") ?? Enumerable.Empty<XElement>(),
                DataTypes = xml.Root.Element("DataTypes")?.Elements("DataType") ?? Enumerable.Empty<XElement>(),
                Languages = xml.Root.Element("Languages")?.Elements("Language") ?? Enumerable.Empty<XElement>(),
                DictionaryItems = xml.Root.Element("DictionaryItems")?.Elements("DictionaryItem") ?? Enumerable.Empty<XElement>(),
                DocumentTypes = xml.Root.Element("DocumentTypes")?.Elements("DocumentType") ?? Enumerable.Empty<XElement>(),
                MediaTypes = xml.Root.Element("MediaTypes")?.Elements("MediaType") ?? Enumerable.Empty<XElement>(),
                Documents = xml.Root.Element("Documents")?.Elements("DocumentSet")?.Select(CompiledPackageContentBase.Create) ?? Enumerable.Empty<CompiledPackageContentBase>(),
                Media = xml.Root.Element("MediaItems")?.Elements()?.Select(CompiledPackageContentBase.Create) ?? Enumerable.Empty<CompiledPackageContentBase>(),
            };

            def.Warnings = GetPreInstallWarnings(def, applicationRootFolder);

            return def;
        }

        private PreInstallWarnings GetPreInstallWarnings(CompiledPackage package, string applicationRootFolder)
        {
            var sourceDestination = ExtractSourceDestinationFileInformation(package.Files);

            var installWarnings = new PreInstallWarnings
            {
                ConflictingMacros = _conflictingPackageData.FindConflictingMacros(package.Macros),
                ConflictingTemplates = _conflictingPackageData.FindConflictingTemplates(package.Templates),
                ConflictingStylesheets = _conflictingPackageData.FindConflictingStylesheets(package.Stylesheets),
                UnsecureFiles = FindUnsecureFiles(sourceDestination),
                FilesReplaced = FindFilesToBeReplaced(sourceDestination, applicationRootFolder)
            };

            return installWarnings;
        }

        /// <summary>
        /// Returns a tuple of the zip file's unique file name and it's application relative path
        /// </summary>
        /// <param name="packageFiles"></param>
        /// <returns></returns>
        public (string packageUniqueFile, string appRelativePath)[] ExtractSourceDestinationFileInformation(IEnumerable<CompiledPackageFile> packageFiles)
        {
            return packageFiles
                .Select(e =>
                {
                    var fileName = PrepareAsFilePathElement(e.OriginalName);
                    var relativeDir = UpdatePathPlaceholders(PrepareAsFilePathElement(e.OriginalPath));
                    var relativePath = Path.Combine(relativeDir, fileName);
                    return (e.UniqueFileName, relativePath);
                }).ToArray();
        }

        private IEnumerable<string> FindFilesToBeReplaced(IEnumerable<(string packageUniqueFile, string appRelativePath)> sourceDestination, string applicationRootFolder)
        {
            return sourceDestination.Where(sd => File.Exists(Path.Combine(applicationRootFolder, sd.appRelativePath)))
                .Select(x => x.appRelativePath)
                .ToArray();
        }

        private IEnumerable<string> FindUnsecureFiles(IEnumerable<(string packageUniqueFile, string appRelativePath)> sourceDestinationPair)
        {
            return sourceDestinationPair.Where(sd => IsFileDestinationUnsecure(sd.appRelativePath))
                .Select(x => x.appRelativePath)
                .ToList();
        }

        private bool IsFileDestinationUnsecure(string destination)
        {
            var unsecureDirNames = new[] { "bin", "app_code" };
            if (unsecureDirNames.Any(ud => destination.StartsWith(ud, StringComparison.InvariantCultureIgnoreCase)))
                return true;

            string extension = Path.GetExtension(destination);
            return extension != null && extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase);
        }

        private static string PrepareAsFilePathElement(string pathElement)
        {
            return pathElement.TrimStart(new[] { '\\', '/', '~' }).Replace('/', Path.DirectorySeparatorChar);
        }

        private string UpdatePathPlaceholders(string path)
        {
            if (path.Contains("[$"))
            {
                //this is experimental and undocumented...
                path = path.Replace("[$UMBRACO]", _globalSettings.UmbracoPath);
                path = path.Replace("[$CONFIG]", Constants.SystemDirectories.Config);
                path = path.Replace("[$DATA]", Constants.SystemDirectories.Data);
            }
            return path;
        }

        /// <summary>
        /// Parses the package actions stored in the package definition
        /// </summary>
        /// <param name="actionsElement"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static IEnumerable<PackageAction> GetPackageActions(XElement actionsElement, string packageName)
        {
            if (actionsElement == null) return Enumerable.Empty<PackageAction>();

            //invariant check ... because people can really enter anything :/
            if (!string.Equals("actions", actionsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                throw new FormatException("Must be \"<actions>\" as root");

            if (!actionsElement.HasElements) return Enumerable.Empty<PackageAction>();

            var actionElementName = actionsElement.Elements().First().Name.LocalName;

            //invariant check ... because people can really enter anything :/
            if (!string.Equals("action", actionElementName, StringComparison.InvariantCultureIgnoreCase))
                throw new FormatException("Must be \"<action\" as element");

            return actionsElement.Elements(actionElementName)
                .Select(e =>
                {
                    var aliasAttr = e.Attribute("alias") ?? e.Attribute("Alias"); //allow both ... because people can really enter anything :/
                    if (aliasAttr == null)
                        throw new ArgumentException("missing \"alias\" attribute in alias element", nameof(actionsElement));

                    var packageAction = new PackageAction
                    {
                        XmlData = e,
                        Alias = aliasAttr.Value,
                        PackageName = packageName,
                    };

                    var attr = e.Attribute("runat") ?? e.Attribute("Runat"); //allow both ... because people can really enter anything :/

                    if (attr != null && Enum.TryParse(attr.Value, true, out ActionRunAt runAt)) { packageAction.RunAt = runAt; }

                    attr = e.Attribute("undo") ?? e.Attribute("Undo"); //allow both ... because people can really enter anything :/

                    if (attr != null && bool.TryParse(attr.Value, out var undo)) { packageAction.Undo = undo; }

                    return packageAction;
                }).ToArray();
        }
    }
}

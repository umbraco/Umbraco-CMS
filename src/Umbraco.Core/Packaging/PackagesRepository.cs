using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Services;
using File = System.IO.File;

namespace Umbraco.Core.Packaging
{
    /// <summary>
    /// Manages the storage of installed/created package definitions
    /// </summary>
    internal class PackagesRepository : ICreatedPackagesRepository, IInstalledPackagesRepository
    {
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IFileService _fileService;
        private readonly IMacroService _macroService;
        private readonly ILocalizationService _languageService;
        private readonly IEntityXmlSerializer _serializer;
        private readonly ILogger _logger;
        private readonly string _packageRepositoryFileName;
        private readonly string _mediaFolderPath;
        private readonly string _packagesFolderPath;
        private readonly string _tempFolderPath;
        private readonly PackageDefinitionXmlParser _parser;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="contentTypeService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="fileService"></param>
        /// <param name="macroService"></param>
        /// <param name="languageService"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        /// <param name="packageRepositoryFileName">
        /// The file name for storing the package definitions (i.e. "createdPackages.config")
        /// </param>
        /// <param name="tempFolderPath"></param>
        /// <param name="packagesFolderPath"></param>
        /// <param name="mediaFolderPath"></param>
        public PackagesRepository(IContentService contentService, IContentTypeService contentTypeService,
            IDataTypeService dataTypeService, IFileService fileService, IMacroService macroService,
            ILocalizationService languageService,
            IEntityXmlSerializer serializer, ILogger logger,
            string packageRepositoryFileName,
            string tempFolderPath = null, string packagesFolderPath = null, string mediaFolderPath = null)
        {
            if (string.IsNullOrWhiteSpace(packageRepositoryFileName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageRepositoryFileName));
            _contentService = contentService;
            _contentTypeService = contentTypeService;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _macroService = macroService;
            _languageService = languageService;
            _serializer = serializer;
            _logger = logger;
            _packageRepositoryFileName = packageRepositoryFileName;

            _tempFolderPath = tempFolderPath ?? SystemDirectories.TempData.EnsureEndsWith('/') + "PackageFiles";
            _packagesFolderPath = packagesFolderPath ?? SystemDirectories.Packages;
            _mediaFolderPath = mediaFolderPath ?? SystemDirectories.Media + "/created-packages";

            _parser = new PackageDefinitionXmlParser(logger);
        }

        private string CreatedPackagesFile => _packagesFolderPath.EnsureEndsWith('/') + _packageRepositoryFileName;

        public IEnumerable<PackageDefinition> GetAll()
        {
            var packagesXml = EnsureStorage(out _);
            if (packagesXml?.Root == null)
                yield break;

            foreach (var packageXml in packagesXml.Root.Elements("package"))
                yield return _parser.ToPackageDefinition(packageXml);
        }

        public PackageDefinition GetById(int id)
        {
            var packagesXml = EnsureStorage(out _);
            var packageXml = packagesXml?.Root?.Elements("package").FirstOrDefault(x => x.AttributeValue<int>("id") == id);
            return packageXml == null ? null : _parser.ToPackageDefinition(packageXml);
        }

        public void Delete(int id)
        {
            var packagesXml = EnsureStorage(out var packagesFile);
            var packageXml = packagesXml?.Root?.Elements("package").FirstOrDefault(x => x.AttributeValue<int>("id") == id);
            if (packageXml == null) return;

            packageXml.Remove();

            packagesXml.Save(packagesFile);
        }

        public bool SavePackage(PackageDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            var packagesXml = EnsureStorage(out var packagesFile);

            if (packagesXml?.Root == null)
                return false;

            //ensure it's valid
            ValidatePackage(definition);

            if (definition.Id == default)
            {
                //need to gen an id and persist
                // Find max id
                var maxId = packagesXml.Root.Elements("package").Max(x => x.AttributeValue<int?>("id")) ?? 0;
                var newId = maxId + 1;
                definition.Id = newId;
                definition.PackageId = definition.PackageId == default ? Guid.NewGuid() : definition.PackageId;
                var packageXml = _parser.ToXml(definition);
                packagesXml.Root.Add(packageXml);
            }
            else
            {
                //existing
                var packageXml = packagesXml.Root.Elements("package").FirstOrDefault(x => x.AttributeValue<int>("id") == definition.Id);
                if (packageXml == null)
                    return false;

                var updatedXml = _parser.ToXml(definition);
                packageXml.ReplaceWith(updatedXml);
            }

            packagesXml.Save(packagesFile);

            return true;
        }

        public string ExportPackage(PackageDefinition definition)
        {
            if (definition.Id == default) throw new ArgumentException("The package definition does not have an ID, it must be saved before being exported");
            if (definition.PackageId == default) throw new ArgumentException("the package definition does not have a GUID, it must be saved before being exported");

            //ensure it's valid
            ValidatePackage(definition);

            //Create a folder for building this package
            var temporaryPath = IOHelper.MapPath(_tempFolderPath.EnsureEndsWith('/') + Guid.NewGuid());
            if (Directory.Exists(temporaryPath) == false)
                Directory.CreateDirectory(temporaryPath);

            try
            {
                //Init package file
                var compiledPackageXml = CreateCompiledPackageXml(out var root, out var filesXml);

                //Info section
                root.Add(GetPackageInfoXml(definition));

                PackageDocumentsAndTags(definition, root);
                PackageDocumentTypes(definition, root);
                PackageTemplates(definition, root);
                PackageStylesheets(definition, root);
                PackageMacros(definition, root, filesXml, temporaryPath);
                PackageDictionaryItems(definition, root);
                PackageLanguages(definition, root);
                PackageDataTypes(definition, root);

                //Files
                foreach (var fileName in definition.Files)
                    AppendFileToPackage(fileName, temporaryPath, filesXml);

                //Load view on install...
                if (!string.IsNullOrEmpty(definition.PackageView))
                {
                    var control = new XElement("view", definition.PackageView);
                    AppendFileToPackage(definition.PackageView, temporaryPath, filesXml);
                    root.Add(control);
                }

                //Actions
                if (string.IsNullOrEmpty(definition.Actions) == false)
                {
                    var actionsXml = new XElement("Actions");
                    try
                    {
                        //this will be formatted like a full xml block like <actions>...</actions> and we want the child nodes
                        var parsed = XElement.Parse(definition.Actions);
                        actionsXml.Add(parsed.Elements());
                        root.Add(actionsXml);
                    }
                    catch (Exception e)
                    {
                        _logger.Warn<PackagesRepository>(e, "Could not add package actions to the package, the xml did not parse");
                    }
                }

                var packageXmlFileName = temporaryPath + "/package.xml";

                if (File.Exists(packageXmlFileName))
                    File.Delete(packageXmlFileName);

                compiledPackageXml.Save(packageXmlFileName);

                // check if there's a packages directory below media

                if (Directory.Exists(IOHelper.MapPath(_mediaFolderPath)) == false)
                    Directory.CreateDirectory(IOHelper.MapPath(_mediaFolderPath));

                var packPath = _mediaFolderPath.EnsureEndsWith('/') + (definition.Name + "_" + definition.Version).Replace(' ', '_') + ".zip";
                ZipPackage(temporaryPath, IOHelper.MapPath(packPath));

                //we need to update the package path and save it
                definition.PackagePath = packPath;
                SavePackage(definition);

                return packPath;
            }
            finally
            {
                //Clean up
                Directory.Delete(temporaryPath, true);
            }
        }

        private void ValidatePackage(PackageDefinition definition)
        {
            //ensure it's valid
            var context = new ValidationContext(definition, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(definition, context, results);
            if (!isValid)
                throw new InvalidOperationException("Validation failed, there is invalid data on the model: " + string.Join(", ", results.Select(x => x.ErrorMessage)));
        }

        private void PackageDataTypes(PackageDefinition definition, XContainer root)
        {
            var dataTypes = new XElement("DataTypes");
            foreach (var dtId in definition.DataTypes)
            {
                if (!int.TryParse(dtId, out var outInt)) continue;
                var dataType = _dataTypeService.GetDataType(outInt);
                if (dataType == null) continue;
                dataTypes.Add(_serializer.Serialize(dataType));
            }
            root.Add(dataTypes);
        }

        private void PackageLanguages(PackageDefinition definition, XContainer root)
        {
            var languages = new XElement("Languages");
            foreach (var langId in definition.Languages)
            {
                if (!int.TryParse(langId, out var outInt)) continue;
                var lang = _languageService.GetLanguageById(outInt);
                if (lang == null) continue;
                languages.Add(_serializer.Serialize(lang));
            }
            root.Add(languages);
        }

        private void PackageDictionaryItems(PackageDefinition definition, XContainer root)
        {
            var dictionaryItems = new XElement("DictionaryItems");
            foreach (var dictionaryId in definition.DictionaryItems)
            {
                if (!int.TryParse(dictionaryId, out var outInt)) continue;
                var di = _languageService.GetDictionaryItemById(outInt);
                if (di == null) continue;
                dictionaryItems.Add(_serializer.Serialize(di, false));
            }
            root.Add(dictionaryItems);
        }

        private void PackageMacros(PackageDefinition definition, XContainer root, XContainer filesXml, string temporaryPath)
        {
            var macros = new XElement("Macros");
            foreach (var macroId in definition.Macros)
            {
                if (!int.TryParse(macroId, out var outInt)) continue;

                var macroXml = GetMacroXml(outInt, out var macro);
                if (macroXml == null) continue;
                macros.Add(macroXml);
                //if the macro has a file copy it to the xml
                if (!string.IsNullOrEmpty(macro.MacroSource))
                    AppendFileToPackage(macro.MacroSource, temporaryPath, filesXml);
            }
            root.Add(macros);
        }

        private void PackageStylesheets(PackageDefinition definition, XContainer root)
        {
            var stylesheetsXml = new XElement("Stylesheets");
            foreach (var stylesheetName in definition.Stylesheets)
            {
                if (stylesheetName.IsNullOrWhiteSpace()) continue;
                var xml = GetStylesheetXml(stylesheetName, true);
                if (xml != null)
                    stylesheetsXml.Add(xml);
            }
            root.Add(stylesheetsXml);
        }

        private void PackageTemplates(PackageDefinition definition, XContainer root)
        {
            var templatesXml = new XElement("Templates");
            foreach (var templateId in definition.Templates)
            {
                if (!int.TryParse(templateId, out var outInt)) continue;
                var template = _fileService.GetTemplate(outInt);
                if (template == null) continue;
                templatesXml.Add(_serializer.Serialize(template));
            }
            root.Add(templatesXml);
        }

        private void PackageDocumentTypes(PackageDefinition definition, XContainer root)
        {
            var contentTypes = new HashSet<IContentType>();
            var docTypesXml = new XElement("DocumentTypes");
            foreach (var dtId in definition.DocumentTypes)
            {
                if (!int.TryParse(dtId, out var outInt)) continue;
                var contentType = _contentTypeService.Get(outInt);
                if (contentType == null) continue;
                AddDocumentType(contentType, contentTypes);
            }
            foreach (var contentType in contentTypes)
                docTypesXml.Add(_serializer.Serialize(contentType));

            root.Add(docTypesXml);
        }

        private void PackageDocumentsAndTags(PackageDefinition definition, XContainer root)
        {
            //Documents and tags
            if (string.IsNullOrEmpty(definition.ContentNodeId) == false && int.TryParse(definition.ContentNodeId, out var contentNodeId))
            {
                if (contentNodeId > 0)
                {
                    //load content from umbraco.
                    var content = _contentService.GetById(contentNodeId);
                    if (content != null)
                    {
                        var contentXml = definition.ContentLoadChildNodes ? content.ToDeepXml(_serializer) : content.ToXml(_serializer);

                        //Create the Documents/DocumentSet node

                        root.Add(
                            new XElement("Documents",
                                new XElement("DocumentSet",
                                    new XAttribute("importMode", "root"),
                                    contentXml)));

                        // TODO: I guess tags has been broken for a very long time for packaging, we should get this working again sometime
                        ////Create the TagProperties node - this is used to store a definition for all
                        //// document properties that are tags, this ensures that we can re-import tags properly
                        //XmlNode tagProps = new XElement("TagProperties");

                        ////before we try to populate this, we'll do a quick lookup to see if any of the documents
                        //// being exported contain published tags.
                        //var allExportedIds = documents.SelectNodes("//@id").Cast<XmlNode>()
                        //    .Select(x => x.Value.TryConvertTo<int>())
                        //    .Where(x => x.Success)
                        //    .Select(x => x.Result)
                        //    .ToArray();
                        //var allContentTags = new List<ITag>();
                        //foreach (var exportedId in allExportedIds)
                        //{
                        //    allContentTags.AddRange(
                        //        Current.Services.TagService.GetTagsForEntity(exportedId));
                        //}

                        ////This is pretty round-about but it works. Essentially we need to get the properties that are tagged
                        //// but to do that we need to lookup by a tag (string)
                        //var allTaggedEntities = new List<TaggedEntity>();
                        //foreach (var group in allContentTags.Select(x => x.Group).Distinct())
                        //{
                        //    allTaggedEntities.AddRange(
                        //        Current.Services.TagService.GetTaggedContentByTagGroup(group));
                        //}

                        ////Now, we have all property Ids/Aliases and their referenced document Ids and tags
                        //var allExportedTaggedEntities = allTaggedEntities.Where(x => allExportedIds.Contains(x.EntityId))
                        //    .DistinctBy(x => x.EntityId)
                        //    .OrderBy(x => x.EntityId);

                        //foreach (var taggedEntity in allExportedTaggedEntities)
                        //{
                        //    foreach (var taggedProperty in taggedEntity.TaggedProperties.Where(x => x.Tags.Any()))
                        //    {
                        //        XmlNode tagProp = new XElement("TagProperty");
                        //        var docId = packageManifest.CreateAttribute("docId", "");
                        //        docId.Value = taggedEntity.EntityId.ToString(CultureInfo.InvariantCulture);
                        //        tagProp.Attributes.Append(docId);

                        //        var propertyAlias = packageManifest.CreateAttribute("propertyAlias", "");
                        //        propertyAlias.Value = taggedProperty.PropertyTypeAlias;
                        //        tagProp.Attributes.Append(propertyAlias);

                        //        var group = packageManifest.CreateAttribute("group", "");
                        //        group.Value = taggedProperty.Tags.First().Group;
                        //        tagProp.Attributes.Append(group);

                        //        tagProp.AppendChild(packageManifest.CreateCDataSection(
                        //            JsonConvert.SerializeObject(taggedProperty.Tags.Select(x => x.Text).ToArray())));

                        //        tagProps.AppendChild(tagProp);
                        //    }
                        //}

                        //manifestRoot.Add(tagProps);
                    }
                }
            }
        }

        /// <summary>
        /// Zips the package.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="savePath">The save path.</param>
        private static void ZipPackage(string path, string savePath)
        {
            if (File.Exists(savePath))
                File.Delete(savePath);
            ZipFile.CreateFromDirectory(path, savePath);
        }

        /// <summary>
        /// Appends a file to package and copies the file to the correct folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="packageDirectory">The package directory.</param>
        /// <param name="filesXml">The files xml node</param>
        private static void AppendFileToPackage(string path, string packageDirectory, XContainer filesXml)
        {
            if (!path.StartsWith("~/") && !path.StartsWith("/"))
                path = "~/" + path;

            var serverPath = IOHelper.MapPath(path);

            if (File.Exists(serverPath))
                AppendFileXml(new FileInfo(serverPath), path, packageDirectory, filesXml);
            else if (Directory.Exists(serverPath))
                ProcessDirectory(new DirectoryInfo(serverPath), path, packageDirectory, filesXml);
        }

        //Process files in directory and add them to package
        private static void ProcessDirectory(DirectoryInfo directory, string dirPath, string packageDirectory, XContainer filesXml)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(packageDirectory)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageDirectory));
            if (string.IsNullOrWhiteSpace(dirPath)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(dirPath));
            if (!directory.Exists) return;

            foreach (var file in directory.GetFiles())
                AppendFileXml(new FileInfo(Path.Combine(directory.FullName, file.Name)), dirPath + "/" + file.Name, packageDirectory, filesXml);

            foreach (var dir in directory.GetDirectories())
                ProcessDirectory(dir, dirPath + "/" + dir.Name, packageDirectory, filesXml);
        }

        private static void AppendFileXml(FileInfo file, string filePath, string packageDirectory, XContainer filesXml)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));
            if (string.IsNullOrWhiteSpace(packageDirectory)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageDirectory));

            var orgPath = filePath.Substring(0, (filePath.LastIndexOf('/')));
            var orgName = filePath.Substring((filePath.LastIndexOf('/') + 1));
            var newFileName = orgName;

            if (File.Exists(packageDirectory.EnsureEndsWith('/') + orgName))
                newFileName = Guid.NewGuid() + "_" + newFileName;

            //Copy file to directory for zipping...
            File.Copy(file.FullName, packageDirectory + "/" + newFileName, true);

            filesXml.Add(new XElement("file",
                new XElement("guid", newFileName),
                new XElement("orgPath", orgPath == "" ? "/" : orgPath),
                new XElement("orgName", orgName)));
        }

        private XElement GetMacroXml(int macroId, out IMacro macro)
        {
            macro = _macroService.GetById(macroId);
            if (macro == null) return null;
            var xml = _serializer.Serialize(macro);
            return xml;
        }

        /// <summary>
        /// Converts a umbraco stylesheet to a package xml node
        /// </summary>
        /// <param name="name">The name of the stylesheet.</param>
        /// <param name="includeProperties">if set to <c>true</c> [include properties].</param>
        /// <returns></returns>
        private XElement GetStylesheetXml(string name, bool includeProperties)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            var sts = _fileService.GetStylesheetByName(name);
            if (sts == null) return null;
            var stylesheetXml = new XElement("Stylesheet");
            stylesheetXml.Add(new XElement("Name", sts.Alias));
            stylesheetXml.Add(new XElement("FileName", sts.Name));
            stylesheetXml.Add(new XElement("Content", new XCData(sts.Content)));

            if (!includeProperties) return stylesheetXml;

            var properties = new XElement("Properties");
            foreach (var ssP in sts.Properties)
            {
                var property = new XElement("Property");
                property.Add(new XElement("Name", ssP.Name));
                property.Add(new XElement("Alias", ssP.Alias));
                property.Add(new XElement("Value", ssP.Value));
            }
            stylesheetXml.Add(properties);
            return stylesheetXml;
        }

        private void AddDocumentType(IContentType dt, HashSet<IContentType> dtl)
        {
            if (dt.ParentId > 0)
            {
                var parent = _contentTypeService.Get(dt.ParentId);
                if (parent != null) // could be a container
                    AddDocumentType(parent, dtl);
            }

            if (!dtl.Contains(dt))
                dtl.Add(dt);
        }

        private static XElement GetPackageInfoXml(PackageDefinition definition)
        {
            var info = new XElement("info");

            //Package info
            var package = new XElement("package");
            package.Add(new XElement("name", definition.Name));
            package.Add(new XElement("version", definition.Version));
            package.Add(new XElement("iconUrl", definition.IconUrl));

            var license = new XElement("license", definition.License);
            license.Add(new XAttribute("url", definition.LicenseUrl));
            package.Add(license);

            package.Add(new XElement("url", definition.Url));

            var requirements = new XElement("requirements");

            requirements.Add(new XElement("major", definition.UmbracoVersion == null ? UmbracoVersion.SemanticVersion.Major.ToInvariantString() : definition.UmbracoVersion.Major.ToInvariantString()));
            requirements.Add(new XElement("minor", definition.UmbracoVersion == null ? UmbracoVersion.SemanticVersion.Minor.ToInvariantString() : definition.UmbracoVersion.Minor.ToInvariantString()));
            requirements.Add(new XElement("patch", definition.UmbracoVersion == null ? UmbracoVersion.SemanticVersion.Patch.ToInvariantString() : definition.UmbracoVersion.Build.ToInvariantString()));

            if (definition.UmbracoVersion != null)
                requirements.Add(new XAttribute("type", RequirementsType.Strict.ToString()));

            package.Add(requirements);
            info.Add(package);

            // Author
            var author = new XElement("author", "");
            author.Add(new XElement("name", definition.Author));
            author.Add(new XElement("website", definition.AuthorUrl));
            info.Add(author);

            // Contributors
            var contributors = new XElement("contributors", "");

            if (definition.Contributors != null && definition.Contributors.Any())
            {
                foreach (var contributor in definition.Contributors)
                {
                    contributors.Add(new XElement("contributor", contributor));
                }
            }

            info.Add(contributors);

            info.Add(new XElement("readme", new XCData(definition.Readme)));

            return info;
        }

        private static XDocument CreateCompiledPackageXml(out XElement root, out XElement files)
        {
            files = new XElement("files");
            root = new XElement("umbPackage", files);
            var compiledPackageXml = new XDocument(root);
            return compiledPackageXml;
        }

        private XDocument EnsureStorage(out string packagesFile)
        {
            var packagesFolder = IOHelper.MapPath(_packagesFolderPath);
            //ensure it exists
            Directory.CreateDirectory(packagesFolder);

            packagesFile = IOHelper.MapPath(CreatedPackagesFile);
            if (!File.Exists(packagesFile))
            {
                var xml = new XDocument(new XElement("packages"));
                xml.Save(packagesFile);
            }

            var packagesXml = XDocument.Load(packagesFile);
            return packagesXml;
        }
    }
}

using System;
using System.Collections.Generic;
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
    internal class PackageCreation : IPackageCreation
    {
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IFileService _fileService;
        private readonly IMacroService _macroService;
        private readonly ILocalizationService _languageService;
        private readonly IEntityXmlSerializer _serializer;
        private readonly ILogger _logger;

        public PackageCreation(IContentService contentService, IContentTypeService contentTypeService,
            IDataTypeService dataTypeService, IFileService fileService, IMacroService macroService,
            ILocalizationService languageService,
            IEntityXmlSerializer serializer, ILogger logger)
        {
            _contentService = contentService;
            _contentTypeService = contentTypeService;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _macroService = macroService;
            _languageService = languageService;
            _serializer = serializer;
            _logger = logger;
        }

        public static string CreatedPackagesFile => SystemDirectories.Packages + IOHelper.DirSepChar + "createdPackages.config";

        public void SavePackageDefinition(PackageDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            var packagesXml = EnsureStorage(out var packagesFile);

            if (definition.Id == default)
            {
                //need to gen an id and persist
                // Find max id
                var maxId = packagesXml.Root.Elements("package").Max(x => x.AttributeValue<int>("id"));
                var newId = maxId + 1;
                definition.Id = newId;
                definition.PackageGuid = Guid.NewGuid().ToString();
                definition.Folder = Guid.NewGuid().ToString();
                var packageXml = PackageDefinitionToXml(definition);
                packagesXml.Add(packageXml);
            }
            else
            {
                //existing
                var packageXml = packagesXml.Root.Elements("package").FirstOrDefault(x => x.AttributeValue<int>("id") == definition.Id);
                if (packageXml == null)
                    throw new InvalidOperationException($"The package with id {definition.Id} was not found");

                var updatedXml = PackageDefinitionToXml(definition);
                packageXml.ReplaceWith(updatedXml);
            }
            
            packagesXml.Save(packagesFile);
        }

        public string ExportPackageDefinition(PackageDefinition definition)
        {
            if (definition.Id == default) throw new ArgumentException("The package definition does not have an ID, it must be saved before being exported");
            if (definition.PackageGuid.IsNullOrWhiteSpace()) throw new ArgumentException("the package definition does not have a GUID, it must be saved before being exported");

            //Create a folder for building this package
            var temporaryPath = IOHelper.MapPath(SystemDirectories.Data + "/TEMP/PackageFiles/" + definition.Folder);
            if (Directory.Exists(temporaryPath) == false)
                Directory.CreateDirectory(temporaryPath);

            try
            {
                //Init package file
                var packageManifest = CreatePackageManifest(out var manifestRoot, out var filesXml);

                //Info section
                packageManifest.Add(GetPackageInfoXml(definition));

                PackageDocumentsAndTags(definition, manifestRoot);
                PackageDocumentTypes(definition, manifestRoot);
                PackageTemplates(definition, manifestRoot);
                PackageStylesheets(definition, manifestRoot);
                PackageMacros(definition, manifestRoot, filesXml, temporaryPath);
                PackageDictionaryItems(definition, manifestRoot);
                PackageLanguages(definition, manifestRoot);
                PackageDataTypes(definition, manifestRoot);

                //Files
                foreach (var fileName in definition.Files)
                    AppendFileToManifest(fileName, temporaryPath, filesXml);

                //Load control on install...
                if (!string.IsNullOrEmpty(definition.LoadControl))
                {
                    var control = new XElement("control", definition.LoadControl);
                    AppendFileToManifest(definition.LoadControl, temporaryPath, filesXml);
                    manifestRoot.Add(control);
                }

                //Actions
                if (string.IsNullOrEmpty(definition.Actions) == false)
                {
                    var actionsXml = new XElement("Actions");
                    try
                    {
                        actionsXml.Add(XElement.Parse(definition.Actions));
                        manifestRoot.Add(actionsXml);
                    }
                    catch (Exception e)
                    {
                        _logger.Warn<PackageCreation>(e, "Could not add package actions to the package manifest, the xml did not parse");
                    }
                }

                var manifestFileName = temporaryPath + "/package.xml";

                if (File.Exists(manifestFileName))
                    File.Delete(manifestFileName);

                packageManifest.Save(manifestFileName);

                // check if there's a packages directory below media
                var packagesDirectory = SystemDirectories.Media + "/created-packages";
                if (Directory.Exists(IOHelper.MapPath(packagesDirectory)) == false)
                    Directory.CreateDirectory(IOHelper.MapPath(packagesDirectory));

                var packPath = packagesDirectory + "/" + (definition.Name + "_" + definition.Version).Replace(' ', '_') + ".zip";
                ZipPackage(temporaryPath, IOHelper.MapPath(packPath));

                return packPath;
            }
            finally
            {
                //Clean up
                Directory.Delete(temporaryPath, true);
            }
        }

        private void PackageDataTypes(PackageDefinition definition, XContainer manifestRoot)
        {
            var dataTypes = new XElement("DataTypes");
            foreach (var dtId in definition.DataTypes)
            {
                if (!int.TryParse(dtId, out var outInt)) continue;
                var dataType = _dataTypeService.GetDataType(outInt);
                if (dataType == null) continue;
                dataTypes.Add(_serializer.Serialize(dataType));
            }
            manifestRoot.Add(dataTypes);
        }

        private void PackageLanguages(PackageDefinition definition, XContainer manifestRoot)
        {
            var languages = new XElement("Languages");
            foreach (var langId in definition.Languages)
            {
                if (!int.TryParse(langId, out var outInt)) continue;
                var lang = _languageService.GetLanguageById(outInt);
                if (lang == null) continue;
                languages.Add(_serializer.Serialize(lang));
            }
            manifestRoot.Add(languages);
        }

        private void PackageDictionaryItems(PackageDefinition definition, XContainer manifestRoot)
        {
            var dictionaryItems = new XElement("DictionaryItems");
            foreach (var dictionaryId in definition.DictionaryItems)
            {
                if (!int.TryParse(dictionaryId, out var outInt)) continue;
                var di = _languageService.GetDictionaryItemById(outInt);
                if (di == null) continue;
                dictionaryItems.Add(_serializer.Serialize(di, false));
            }
            manifestRoot.Add(dictionaryItems);
        }

        private void PackageMacros(PackageDefinition definition, XContainer manifestRoot, XContainer filesXml, string temporaryPath)
        {
            var macros = new XElement("Macros");
            foreach (var macroId in definition.Macros)
            {
                if (!int.TryParse(macroId, out var outInt)) continue;

                var macroXml = GetMacroXml(outInt, out var macro);
                if (macroXml == null) continue;
                macros.Add(macroXml);
                //if the macro has a file copy it to the manifest
                if (!string.IsNullOrEmpty(macro.MacroSource))
                    AppendFileToManifest(macro.MacroSource, temporaryPath, filesXml);
            }
            manifestRoot.Add(macros);
        }

        private void PackageStylesheets(PackageDefinition definition, XContainer manifestRoot)
        {
            var stylesheetsXml = new XElement("Stylesheets");
            foreach (var stylesheetName in definition.Stylesheets)
            {
                if (stylesheetName.IsNullOrWhiteSpace()) continue;
                var xml = GetStylesheetXml(stylesheetName, true);
                if (xml != null)
                    stylesheetsXml.Add(xml);
            }
            manifestRoot.Add(stylesheetsXml);
        }

        private void PackageTemplates(PackageDefinition definition, XContainer manifestRoot)
        {
            var templatesXml = new XElement("Templates");
            foreach (var templateId in definition.Templates)
            {
                if (!int.TryParse(templateId, out var outInt)) continue;
                var template = _fileService.GetTemplate(outInt);
                if (template == null) continue;
                templatesXml.Add(_serializer.Serialize(template));
            }
            manifestRoot.Add(templatesXml);
        }

        private void PackageDocumentTypes(PackageDefinition definition, XContainer manifestRoot)
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

            manifestRoot.Add(docTypesXml);
        }

        private void PackageDocumentsAndTags(PackageDefinition definition, XContainer manifestRoot)
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

                        manifestRoot.Add(
                            new XElement("Documents",
                                new XElement("DocumentSet",
                                    new XAttribute("importMode", "root"),
                                    contentXml)));

                        //TODO: I guess tags has been broken for a very long time for packaging, we should get this working again sometime
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
            ZipFile.CreateFromDirectory(path, savePath);
        }

        /// <summary>
        /// Appends a file to package manifest and copies the file to the correct folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="packageDirectory">The package directory.</param>
        /// <param name="filesXml">The files xml node</param>
        private static void AppendFileToManifest(string path, string packageDirectory, XContainer filesXml)
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
        /// <param name="includeProperties">if set to <c>true</c> [incluce properties].</param>
        /// <returns></returns>
        private XElement GetStylesheetXml(string name, bool includeProperties)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
;
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
                requirements.Add(new XAttribute("type", "strict"));

            package.Add(requirements);
            info.Add(package);

            //Author
            var author = new XElement("author", "");
            author.Add(new XElement("name", definition.Author));
            author.Add(new XElement("website", definition.AuthorUrl));
            info.Add(author);

            info.Add(new XElement("readme", new XCData(definition.Readme)));

            return info;
        }

        private static XDocument CreatePackageManifest(out XElement root, out XElement files)
        {
            files = new XElement("files");
            root = new XElement("umbPackage", files);
            var packageManifest = new XDocument();
            return packageManifest;
        }

        private static XDocument EnsureStorage(out string packagesFile)
        {
            var packagesFolder = IOHelper.MapPath(SystemDirectories.Packages);
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

        private static XElement PackageDefinitionToXml(PackageDefinition def)
        {
            var packageXml = new XElement("package",
                new XAttribute("id", def.Id),
                new XAttribute("version", def.Version),
                new XAttribute("url", def.Url),
                new XAttribute("name", def.Name),
                new XAttribute("folder", def.Folder), //fixme: What is this?
                new XAttribute("packagepath", def.PackagePath),
                new XAttribute("repositoryGuid", def.RepositoryGuid),
                new XAttribute("iconUrl", def.IconUrl),
                new XAttribute("umbVersion", def.UmbracoVersion),
                new XAttribute("packageGuid", def.PackageGuid),
                new XAttribute("hasUpdate", def.HasUpdate), //fixme: What is this?

                new XElement("license",
                    new XCData(def.License),
                    new XAttribute("url", def.LicenseUrl)),

                new XElement("author",
                    new XCData(def.Author),
                    new XAttribute("url", def.AuthorUrl)),

                new XElement("readme", def.Readme),
                new XElement("actions", def.Actions),
                new XElement("datatypes", string.Join(",", def.DataTypes)),

                new XElement("content",
                    new XAttribute("nodeId", def.ContentNodeId),
                    new XAttribute("loadChildNodes", def.ContentLoadChildNodes)),

                new XElement("templates", string.Join(",", def.Templates)),
                new XElement("stylesheets", string.Join(",", def.Stylesheets)),
                new XElement("documentTypes", string.Join(",", def.DocumentTypes)),
                new XElement("macros", string.Join(",", def.Macros)),
                new XElement("files", string.Join(",", def.Files)),
                new XElement("languages", string.Join(",", def.Languages)),
                new XElement("dictionaryitems", string.Join(",", def.DictionaryItems)),
                new XElement("loadcontrol", "")); //fixme: no more loadcontrol, needs to be an angular view

            return packageXml;
        }

    }
}

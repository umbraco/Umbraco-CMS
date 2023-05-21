using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Manages the storage of installed/created package definitions
/// </summary>
[Obsolete(
    "Packages have now been moved to the database instead of local files, please use CreatedPackageSchemaRepository instead")]
public class PackagesRepository : ICreatedPackagesRepository
{
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly string _createdPackagesFolderPath;
    private readonly IDataTypeService _dataTypeService;
    private readonly IFileService _fileService;
    private readonly FileSystems _fileSystems;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILocalizationService _languageService;
    private readonly IMacroService _macroService;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly string _packageRepositoryFileName;
    private readonly string _packagesFolderPath;
    private readonly PackageDefinitionXmlParser _parser;
    private readonly IEntityXmlSerializer _serializer;
    private readonly string _tempFolderPath;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="contentTypeService"></param>
    /// <param name="dataTypeService"></param>
    /// <param name="fileService"></param>
    /// <param name="macroService"></param>
    /// <param name="languageService"></param>
    /// <param name="hostingEnvironment"></param>
    /// <param name="serializer"></param>
    /// <param name="globalSettings"></param>
    /// <param name="packageRepositoryFileName">
    ///     The file name for storing the package definitions (i.e. "createdPackages.config")
    /// </param>
    /// <param name="tempFolderPath"></param>
    /// <param name="packagesFolderPath"></param>
    /// <param name="mediaFolderPath"></param>
    /// <param name="mediaService"></param>
    /// <param name="mediaTypeService"></param>
    /// <param name="mediaFileManager"></param>
    /// <param name="fileSystems"></param>
    public PackagesRepository(
        IContentService contentService,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IFileService fileService,
        IMacroService macroService,
        ILocalizationService languageService,
        IHostingEnvironment hostingEnvironment,
        IEntityXmlSerializer serializer,
        IOptions<GlobalSettings> globalSettings,
        IMediaService mediaService,
        IMediaTypeService mediaTypeService,
        MediaFileManager mediaFileManager,
        FileSystems fileSystems,
        string packageRepositoryFileName,
        string? tempFolderPath = null,
        string? packagesFolderPath = null,
        string? mediaFolderPath = null)
    {
        if (string.IsNullOrWhiteSpace(packageRepositoryFileName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageRepositoryFileName));
        }

        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
        _fileService = fileService;
        _macroService = macroService;
        _languageService = languageService;
        _serializer = serializer;
        _hostingEnvironment = hostingEnvironment;
        _packageRepositoryFileName = packageRepositoryFileName;

        _tempFolderPath = tempFolderPath ?? Constants.SystemDirectories.TempData.EnsureEndsWith('/') + "PackageFiles";
        _packagesFolderPath = packagesFolderPath ?? Constants.SystemDirectories.Packages;
        _createdPackagesFolderPath = mediaFolderPath ?? Constants.SystemDirectories.CreatedPackages;

        _parser = new PackageDefinitionXmlParser();
        _mediaService = mediaService;
        _mediaTypeService = mediaTypeService;
        _mediaFileManager = mediaFileManager;
        _fileSystems = fileSystems;
    }

    private string CreatedPackagesFile => _packagesFolderPath.EnsureEndsWith('/') + _packageRepositoryFileName;

    public IEnumerable<PackageDefinition?> GetAll()
    {
        XDocument packagesXml = EnsureStorage(out _);
        if (packagesXml?.Root == null)
        {
            yield break;
        }

        foreach (XElement packageXml in packagesXml.Root.Elements("package"))
        {
            yield return _parser.ToPackageDefinition(packageXml);
        }
    }

    public PackageDefinition? GetById(int id)
    {
        XDocument packagesXml = EnsureStorage(out var packageFile);
        XElement? packageXml = packagesXml?.Root?.Elements("package")
            .FirstOrDefault(x => x.AttributeValue<int>("id") == id);
        return packageXml == null ? null : _parser.ToPackageDefinition(packageXml);
    }

    public void Delete(int id)
    {
        XDocument packagesXml = EnsureStorage(out var packagesFile);
        XElement? packageXml = packagesXml?.Root?.Elements("package")
            .FirstOrDefault(x => x.AttributeValue<int>("id") == id);
        if (packageXml == null)
        {
            return;
        }

        packageXml.Remove();

        packagesXml?.Save(packagesFile);
    }

    public bool SavePackage(PackageDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        XDocument packagesXml = EnsureStorage(out var packagesFile);

        if (packagesXml?.Root == null)
        {
            return false;
        }

        // ensure it's valid
        ValidatePackage(definition);

        if (definition.Id == default)
        {
            // need to gen an id and persist
            // Find max id
            var maxId = packagesXml.Root.Elements("package").Max(x => x.AttributeValue<int?>("id")) ?? 0;
            var newId = maxId + 1;
            definition.Id = newId;
            definition.PackageId = definition.PackageId == default ? Guid.NewGuid() : definition.PackageId;
            XElement packageXml = _parser.ToXml(definition);
            packagesXml.Root.Add(packageXml);
        }
        else
        {
            // existing
            XElement? packageXml = packagesXml.Root.Elements("package")
                .FirstOrDefault(x => x.AttributeValue<int>("id") == definition.Id);
            if (packageXml == null)
            {
                return false;
            }

            XElement updatedXml = _parser.ToXml(definition);
            packageXml.ReplaceWith(updatedXml);
        }

        packagesXml.Save(packagesFile);

        return true;
    }

    public string ExportPackage(PackageDefinition definition)
    {
        if (definition.Id == default)
        {
            throw new ArgumentException(
                "The package definition does not have an ID, it must be saved before being exported");
        }

        if (definition.PackageId == default)
        {
            throw new ArgumentException(
                "the package definition does not have a GUID, it must be saved before being exported");
        }

        // ensure it's valid
        ValidatePackage(definition);

        // Create a folder for building this package
        var temporaryPath =
            _hostingEnvironment.MapPathContentRoot(_tempFolderPath.EnsureEndsWith('/') + Guid.NewGuid());
        if (Directory.Exists(temporaryPath) == false)
        {
            Directory.CreateDirectory(temporaryPath);
        }

        try
        {
            // Init package file
            XDocument compiledPackageXml = CreateCompiledPackageXml(out XElement root);

            // Info section
            root.Add(GetPackageInfoXml(definition));

            PackageDocumentsAndTags(definition, root);
            PackageDocumentTypes(definition, root);
            PackageMediaTypes(definition, root);
            PackageTemplates(definition, root);
            PackageStylesheets(definition, root);
            PackageStaticFiles(definition.Scripts, root, "Scripts", "Script", _fileSystems.ScriptsFileSystem);
            PackageStaticFiles(definition.PartialViews, root, "PartialViews", "View", _fileSystems.PartialViewsFileSystem);
            PackageMacros(definition, root);
            PackageDictionaryItems(definition, root);
            PackageLanguages(definition, root);
            PackageDataTypes(definition, root);
            Dictionary<string, Stream> mediaFiles = PackageMedia(definition, root);

            string fileName;
            string tempPackagePath;
            if (mediaFiles.Count > 0)
            {
                fileName = "package.zip";
                tempPackagePath = Path.Combine(temporaryPath, fileName);
                using (FileStream fileStream = File.OpenWrite(tempPackagePath))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry packageXmlEntry = archive.CreateEntry("package.xml");
                    using (Stream entryStream = packageXmlEntry.Open())
                    {
                        compiledPackageXml.Save(entryStream);
                    }

                    foreach (KeyValuePair<string, Stream> mediaFile in mediaFiles)
                    {
                        var entryPath = $"media{mediaFile.Key.EnsureStartsWith('/')}";
                        ZipArchiveEntry mediaEntry = archive.CreateEntry(entryPath);
                        using (Stream entryStream = mediaEntry.Open())
                        using (mediaFile.Value)
                        {
                            mediaFile.Value.Seek(0, SeekOrigin.Begin);
                            mediaFile.Value.CopyTo(entryStream);
                        }
                    }
                }
            }
            else
            {
                fileName = "package.xml";
                tempPackagePath = Path.Combine(temporaryPath, fileName);

                using (FileStream fileStream = File.OpenWrite(tempPackagePath))
                {
                    compiledPackageXml.Save(fileStream);
                }
            }

            var directoryName =
                _hostingEnvironment.MapPathContentRoot(Path.Combine(
                    _createdPackagesFolderPath,
                    definition.Name.Replace(' ', '_')));
            Directory.CreateDirectory(directoryName);

            var finalPackagePath = Path.Combine(directoryName, fileName);

            if (File.Exists(finalPackagePath))
            {
                File.Delete(finalPackagePath);
            }

            File.Move(tempPackagePath, finalPackagePath);

            definition.PackagePath = finalPackagePath;
            SavePackage(definition);

            return finalPackagePath;
        }
        finally
        {
            // Clean up
            Directory.Delete(temporaryPath, true);
        }
    }

    public void DeleteLocalRepositoryFiles()
    {
        var packagesFile = _hostingEnvironment.MapPathContentRoot(CreatedPackagesFile);
        if (File.Exists(packagesFile))
        {
            File.Delete(packagesFile);
        }

        var packagesFolder = _hostingEnvironment.MapPathContentRoot(_packagesFolderPath);
        if (Directory.Exists(packagesFolder))
        {
            Directory.Delete(packagesFolder);
        }
    }

    private static XElement GetPackageInfoXml(PackageDefinition definition)
    {
        var info = new XElement("info");

        // Package info
        var package = new XElement("package");
        package.Add(new XElement("name", definition.Name));
        info.Add(package);
        return info;
    }

    private void ValidatePackage(PackageDefinition definition)
    {
        // ensure it's valid
        var context = new ValidationContext(definition, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(definition, context, results);
        if (!isValid)
        {
            throw new InvalidOperationException("Validation failed, there is invalid data on the model: " +
                                                string.Join(", ", results.Select(x => x.ErrorMessage)));
        }
    }

    private void PackageDataTypes(PackageDefinition definition, XContainer root)
    {
        var dataTypes = new XElement("DataTypes");
        foreach (var dtId in definition.DataTypes)
        {
            if (!int.TryParse(dtId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outInt))
            {
                continue;
            }

            IDataType? dataType = _dataTypeService.GetDataType(outInt);
            if (dataType == null)
            {
                continue;
            }

            dataTypes.Add(_serializer.Serialize(dataType));
        }

        root.Add(dataTypes);
    }

    private void PackageLanguages(PackageDefinition definition, XContainer root)
    {
        var languages = new XElement("Languages");
        foreach (var langId in definition.Languages)
        {
            if (!int.TryParse(langId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outInt))
            {
                continue;
            }

            ILanguage? lang = _languageService.GetLanguageById(outInt);
            if (lang == null)
            {
                continue;
            }

            languages.Add(_serializer.Serialize(lang));
        }

        root.Add(languages);
    }

    private void PackageDictionaryItems(PackageDefinition definition, XContainer root)
    {
        var rootDictionaryItems = new XElement("DictionaryItems");
        var items = new Dictionary<Guid, (IDictionaryItem dictionaryItem, XElement serializedDictionaryValue)>();

        foreach (var dictionaryId in definition.DictionaryItems)
        {
            if (!int.TryParse(dictionaryId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outInt))
            {
                continue;
            }

            IDictionaryItem? di = _languageService.GetDictionaryItemById(outInt);

            if (di == null)
            {
                continue;
            }

            items[di.Key] = (di, _serializer.Serialize(di, false));
        }

        // organize them in hierarchy ...
        var itemCount = items.Count;
        var processed = new Dictionary<Guid, XElement>();
        while (processed.Count < itemCount)
        {
            foreach (Guid key in items.Keys.ToList())
            {
                (IDictionaryItem dictionaryItem, XElement serializedDictionaryValue) = items[key];

                if (!dictionaryItem.ParentId.HasValue)
                {
                    // if it has no parent, its definitely just at the root
                    AppendDictionaryElement(rootDictionaryItems, items, processed, key, serializedDictionaryValue);
                }
                else
                {
                    if (processed.ContainsKey(dictionaryItem.ParentId.Value))
                    {
                        // we've processed this parent element already so we can just append this xml child to it
                        AppendDictionaryElement(processed[dictionaryItem.ParentId.Value], items, processed, key, serializedDictionaryValue);
                    }
                    else if (items.ContainsKey(dictionaryItem.ParentId.Value))
                    {
                        // we know the parent exists in the dictionary but
                        // we haven't processed it yet so we'll leave it for the next loop
                    }
                    else
                    {
                        // in this case, the parent of this item doesn't exist in our collection, we have no
                        // choice but to add it to the root.
                        AppendDictionaryElement(rootDictionaryItems, items, processed, key, serializedDictionaryValue);
                    }
                }
            }
        }

        root.Add(rootDictionaryItems);

        static void AppendDictionaryElement(
            XElement rootDictionaryItems,
            Dictionary<Guid, (IDictionaryItem dictionaryItem, XElement serializedDictionaryValue)> items,
            Dictionary<Guid, XElement> processed,
            Guid key,
            XElement serializedDictionaryValue)
        {
            // track it
            processed.Add(key, serializedDictionaryValue);

            // append it
            rootDictionaryItems.Add(serializedDictionaryValue);

            // remove it so its not re-processed
            items.Remove(key);
        }
    }

    private void PackageMacros(PackageDefinition definition, XContainer root)
    {
        var packagedMacros = new List<IMacro>();
        var macros = new XElement("Macros");
        foreach (var macroId in definition.Macros)
        {
            if (!int.TryParse(macroId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outInt))
            {
                continue;
            }

            XElement? macroXml = GetMacroXml(outInt, out IMacro? macro);
            if (macroXml == null)
            {
                continue;
            }

            macros.Add(macroXml);
            if (macro is not null)
            {
                packagedMacros.Add(macro);
            }
        }

        root.Add(macros);

        // Get the partial views for macros and package those (exclude views outside of the default directory, e.g. App_Plugins\*\Views)
        IEnumerable<string> views = packagedMacros.Where(x => x.MacroSource is not null)
            .Where(x => x.MacroSource!.StartsWith(Constants.SystemDirectories.MacroPartials))
            .Select(x => x.MacroSource![Constants.SystemDirectories.MacroPartials.Length..].Replace('/', '\\'));
        PackageStaticFiles(views, root, "MacroPartialViews", "View", _fileSystems.MacroPartialsFileSystem);
    }

    private void PackageStylesheets(PackageDefinition definition, XContainer root)
    {
        var stylesheetsXml = new XElement("Stylesheets");
        foreach (var stylesheet in definition.Stylesheets)
        {
            if (stylesheet.IsNullOrWhiteSpace())
            {
                continue;
            }

            XElement? xml = GetStylesheetXml(stylesheet, true);
            if (xml is not null)
            {
                stylesheetsXml.Add(xml);
            }
        }

        root.Add(stylesheetsXml);
    }

    private void PackageStaticFiles(
        IEnumerable<string> filePaths,
        XContainer root,
        string containerName,
        string elementName,
        IFileSystem? fileSystem)
    {
        var scriptsXml = new XElement(containerName);
        foreach (var file in filePaths)
        {
            if (file.IsNullOrWhiteSpace())
            {
                continue;
            }

            if (!fileSystem?.FileExists(file) ?? false)
            {
                throw new InvalidOperationException("No file found with path " + file);
            }

            using (Stream stream = fileSystem!.OpenFile(file))
            {
                using (var reader = new StreamReader(stream))
                {
                    var fileContents = reader.ReadToEnd();
                    scriptsXml.Add(
                        new XElement(
                            elementName,
                            new XAttribute("path", file),
                            new XCData(fileContents)));
                }
            }
        }

        root.Add(scriptsXml);
    }

    private void PackageTemplates(PackageDefinition definition, XContainer root)
    {
        var templatesXml = new XElement("Templates");
        foreach (var templateId in definition.Templates)
        {
            if (!int.TryParse(templateId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outInt))
            {
                continue;
            }

            ITemplate? template = _fileService.GetTemplate(outInt);
            if (template == null)
            {
                continue;
            }

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
            if (!int.TryParse(dtId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outInt))
            {
                continue;
            }

            IContentType? contentType = _contentTypeService.Get(outInt);
            if (contentType == null)
            {
                continue;
            }

            AddDocumentType(contentType, contentTypes);
        }

        foreach (IContentType contentType in contentTypes)
        {
            docTypesXml.Add(_serializer.Serialize(contentType));
        }

        root.Add(docTypesXml);
    }

    private void PackageMediaTypes(PackageDefinition definition, XContainer root)
    {
        var mediaTypes = new HashSet<IMediaType>();
        var mediaTypesXml = new XElement("MediaTypes");
        foreach (var mediaTypeId in definition.MediaTypes)
        {
            if (!int.TryParse(mediaTypeId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outInt))
            {
                continue;
            }

            IMediaType? mediaType = _mediaTypeService.Get(outInt);
            if (mediaType == null)
            {
                continue;
            }

            AddMediaType(mediaType, mediaTypes);
        }

        foreach (IMediaType mediaType in mediaTypes)
        {
            mediaTypesXml.Add(_serializer.Serialize(mediaType));
        }

        root.Add(mediaTypesXml);
    }

    private void PackageDocumentsAndTags(PackageDefinition definition, XContainer root)
    {
        // Documents and tags
        if (string.IsNullOrEmpty(definition.ContentNodeId) == false && int.TryParse(definition.ContentNodeId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var contentNodeId))
        {
            if (contentNodeId > 0)
            {
                // load content from umbraco.
                IContent? content = _contentService.GetById(contentNodeId);
                if (content != null)
                {
                    XElement contentXml = definition.ContentLoadChildNodes
                        ? content.ToDeepXml(_serializer)
                        : content.ToXml(_serializer);

                    // Create the Documents/DocumentSet node
                    root.Add(
                        new XElement(
                            "Documents",
                            new XElement(
                                "DocumentSet",
                                new XAttribute("importMode", "root"),
                                contentXml)));

                    // TODO: I guess tags has been broken for a very long time for packaging, we should get this working again sometime
                    ////Create the TagProperties node - this is used to store a definition for all
                    //// document properties that are tags, this ensures that we can re-import tags properly
                    // XmlNode tagProps = new XElement("TagProperties");

                    ////before we try to populate this, we'll do a quick lookup to see if any of the documents
                    //// being exported contain published tags.
                    // var allExportedIds = documents.SelectNodes("//@id").Cast<XmlNode>()
                    //    .Select(x => x.Value.TryConvertTo<int>())
                    //    .Where(x => x.Success)
                    //    .Select(x => x.Result)
                    //    .ToArray();
                    // var allContentTags = new List<ITag>();
                    // foreach (var exportedId in allExportedIds)
                    // {
                    //    allContentTags.AddRange(
                    //        Current.Services.TagService.GetTagsForEntity(exportedId));
                    // }

                    ////This is pretty round-about but it works. Essentially we need to get the properties that are tagged
                    //// but to do that we need to lookup by a tag (string)
                    // var allTaggedEntities = new List<TaggedEntity>();
                    // foreach (var group in allContentTags.Select(x => x.Group).Distinct())
                    // {
                    //    allTaggedEntities.AddRange(
                    //        Current.Services.TagService.GetTaggedContentByTagGroup(group));
                    // }

                    ////Now, we have all property Ids/Aliases and their referenced document Ids and tags
                    // var allExportedTaggedEntities = allTaggedEntities.Where(x => allExportedIds.Contains(x.EntityId))
                    //    .DistinctBy(x => x.EntityId)
                    //    .OrderBy(x => x.EntityId);

                    // foreach (var taggedEntity in allExportedTaggedEntities)
                    // {
                    //    foreach (var taggedProperty in taggedEntity.TaggedProperties.Where(x => x.Tags.Any()))
                    //    {
                    //        XmlNode tagProp = new XElement("TagProperty");
                    //        var docId = packageManifest.CreateAttribute("docId", "");
                    //        docId.Value = taggedEntity.EntityId.ToString(CultureInfo.InvariantCulture);
                    //        tagProp.Attributes.Append(docId);

                    // var propertyAlias = packageManifest.CreateAttribute("propertyAlias", "");
                    //        propertyAlias.Value = taggedProperty.PropertyTypeAlias;
                    //        tagProp.Attributes.Append(propertyAlias);

                    // var group = packageManifest.CreateAttribute("group", "");
                    //        group.Value = taggedProperty.Tags.First().Group;
                    //        tagProp.Attributes.Append(group);

                    // tagProp.AppendChild(packageManifest.CreateCDataSection(
                    //            JsonConvert.SerializeObject(taggedProperty.Tags.Select(x => x.Text).ToArray())));

                    // tagProps.AppendChild(tagProp);
                    //    }
                    // }

                    // manifestRoot.Add(tagProps);
                }
            }
        }
    }

    private Dictionary<string, Stream> PackageMedia(PackageDefinition definition, XElement root)
    {
        var mediaStreams = new Dictionary<string, Stream>();

        // callback that occurs on each serialized media item
        void OnSerializedMedia(IMedia media, XElement xmlMedia)
        {
            // get the media file path and store that separately in the XML.
            // the media file path is different from the URL and is specifically
            // extracted using the property editor for this media file and the current media file system.
            Stream? mediaStream = _mediaFileManager.GetFile(media, out var mediaFilePath);
            if (mediaStream != null && mediaFilePath is not null)
            {
                xmlMedia.Add(new XAttribute("mediaFilePath", mediaFilePath));

                // add the stream to our outgoing stream
                mediaStreams.Add(mediaFilePath, mediaStream);
            }
        }

        IEnumerable<IMedia> medias = _mediaService.GetByIds(definition.MediaUdis);

        var mediaXml = new XElement(
            "MediaItems",
            medias.Select(media =>
            {
                XElement serializedMedia = _serializer.Serialize(
                    media,
                    definition.MediaLoadChildNodes,
                    OnSerializedMedia);

                return new XElement("MediaSet", serializedMedia);
            }));

        root.Add(mediaXml);

        return mediaStreams;
    }

    // TODO: Delete this
    private XElement? GetMacroXml(int macroId, out IMacro? macro)
    {
        macro = _macroService.GetById(macroId);
        if (macro == null)
        {
            return null;
        }

        XElement xml = _serializer.Serialize(macro);
        return xml;
    }

    /// <summary>
    ///     Converts a umbraco stylesheet to a package xml node
    /// </summary>
    /// <param name="path">The path of the stylesheet.</param>
    /// <param name="includeProperties">if set to <c>true</c> [include properties].</param>
    /// <returns></returns>
    private XElement? GetStylesheetXml(string path, bool includeProperties)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
        }

        IStylesheet? stylesheet = _fileService.GetStylesheet(path);
        if (stylesheet == null)
        {
            return null;
        }

        return _serializer.Serialize(stylesheet, includeProperties);
    }

    private void AddDocumentType(IContentType dt, HashSet<IContentType> dtl)
    {
        if (dt.ParentId > 0)
        {
            IContentType? parent = _contentTypeService.Get(dt.ParentId);

            // could be a container
            if (parent != null)
            {
                AddDocumentType(parent, dtl);
            }
        }

        if (!dtl.Contains(dt))
        {
            dtl.Add(dt);
        }
    }

    private void AddMediaType(IMediaType mediaType, HashSet<IMediaType> mediaTypes)
    {
        if (mediaType.ParentId > 0)
        {
            IMediaType? parent = _mediaTypeService.Get(mediaType.ParentId);

            // could be a container
            if (parent != null)
            {
                AddMediaType(parent, mediaTypes);
            }
        }

        if (!mediaTypes.Contains(mediaType))
        {
            mediaTypes.Add(mediaType);
        }
    }

    private static XDocument CreateCompiledPackageXml(out XElement root)
    {
        root = new XElement("umbPackage");
        var compiledPackageXml = new XDocument(root);
        return compiledPackageXml;
    }

    private XDocument EnsureStorage(out string packagesFile)
    {
        var packagesFolder = _hostingEnvironment.MapPathContentRoot(_packagesFolderPath);
        Directory.CreateDirectory(packagesFolder);

        packagesFile = _hostingEnvironment.MapPathContentRoot(CreatedPackagesFile);
        if (!File.Exists(packagesFile))
        {
            var xml = new XDocument(new XElement("packages"));
            xml.Save(packagesFile);

            return xml;
        }

        var packagesXml = XDocument.Load(packagesFile);
        return packagesXml;
    }
}

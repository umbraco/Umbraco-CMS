using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc />
public class CreatedPackageSchemaRepository : ICreatedPackagesRepository
{
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly string _createdPackagesFolderPath;
    private readonly IDataTypeService _dataTypeService;
    private readonly IFileService _fileService;
    private readonly FileSystems _fileSystems;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILocalizationService _localizationService;
    private readonly IMacroService _macroService;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IEntityXmlSerializer _serializer;
    private readonly string _tempFolderPath;
    private readonly IUmbracoDatabase? _umbracoDatabase;
    private readonly PackageDefinitionXmlParser _xmlParser;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreatedPackageSchemaRepository" /> class.
    /// </summary>
    public CreatedPackageSchemaRepository(
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IHostingEnvironment hostingEnvironment,
        IOptions<GlobalSettings> globalSettings,
        FileSystems fileSystems,
        IEntityXmlSerializer serializer,
        IDataTypeService dataTypeService,
        ILocalizationService localizationService,
        IFileService fileService,
        IMediaService mediaService,
        IMediaTypeService mediaTypeService,
        IContentService contentService,
        MediaFileManager mediaFileManager,
        IMacroService macroService,
        IContentTypeService contentTypeService,
        string? mediaFolderPath = null,
        string? tempFolderPath = null)
    {
        _umbracoDatabase = umbracoDatabaseFactory.CreateDatabase();
        _hostingEnvironment = hostingEnvironment;
        _fileSystems = fileSystems;
        _serializer = serializer;
        _dataTypeService = dataTypeService;
        _localizationService = localizationService;
        _fileService = fileService;
        _mediaService = mediaService;
        _mediaTypeService = mediaTypeService;
        _contentService = contentService;
        _mediaFileManager = mediaFileManager;
        _macroService = macroService;
        _contentTypeService = contentTypeService;
        _xmlParser = new PackageDefinitionXmlParser();
        _createdPackagesFolderPath = mediaFolderPath ?? Constants.SystemDirectories.CreatedPackages;
        _tempFolderPath = tempFolderPath ?? Constants.SystemDirectories.TempData + "/PackageFiles";
    }

    public IEnumerable<PackageDefinition> GetAll()
    {
        Sql<ISqlContext> query = new Sql<ISqlContext>(_umbracoDatabase!.SqlContext)
            .Select<CreatedPackageSchemaDto>()
            .From<CreatedPackageSchemaDto>()
            .OrderBy<CreatedPackageSchemaDto>(x => x.Id);

        var packageDefinitions = new List<PackageDefinition>();

        List<CreatedPackageSchemaDto> xmlSchemas = _umbracoDatabase.Fetch<CreatedPackageSchemaDto>(query);
        foreach (CreatedPackageSchemaDto packageSchema in xmlSchemas)
        {
            var packageDefinition = _xmlParser.ToPackageDefinition(XElement.Parse(packageSchema.Value));
            if (packageDefinition is not null)
            {
                packageDefinition.Id = packageSchema.Id;
                packageDefinition.Name = packageSchema.Name;
                packageDefinition.PackageId = packageSchema.PackageId;
                packageDefinitions.Add(packageDefinition);
            }
        }

        return packageDefinitions;
    }

    public PackageDefinition? GetById(int id)
    {
        Sql<ISqlContext> query = new Sql<ISqlContext>(_umbracoDatabase!.SqlContext)
            .Select<CreatedPackageSchemaDto>()
            .From<CreatedPackageSchemaDto>()
            .Where<CreatedPackageSchemaDto>(x => x.Id == id);
        List<CreatedPackageSchemaDto> schemaDtos = _umbracoDatabase.Fetch<CreatedPackageSchemaDto>(query);

        if (schemaDtos.IsCollectionEmpty())
        {
            return null;
        }

        CreatedPackageSchemaDto packageSchema = schemaDtos.First();
        var packageDefinition = _xmlParser.ToPackageDefinition(XElement.Parse(packageSchema.Value));
        if (packageDefinition is not null)
        {
            packageDefinition.Id = packageSchema.Id;
            packageDefinition.Name = packageSchema.Name;
            packageDefinition.PackageId = packageSchema.PackageId;
        }

        return packageDefinition;
    }

    public void Delete(int id)
    {
        // Delete package snapshot
        PackageDefinition? packageDef = GetById(id);
        if (File.Exists(packageDef?.PackagePath))
        {
            File.Delete(packageDef.PackagePath);
        }

        Sql<ISqlContext> query = new Sql<ISqlContext>(_umbracoDatabase!.SqlContext)
            .Delete<CreatedPackageSchemaDto>()
            .Where<CreatedPackageSchemaDto>(x => x.Id == id);

        _umbracoDatabase.Execute(query);
    }

    public bool SavePackage(PackageDefinition? definition)
    {
        if (definition == null)
        {
            throw new NullReferenceException("PackageDefinition cannot be null when saving");
        }

        if (string.IsNullOrEmpty(definition.Name) || definition.PackagePath == null)
        {
            return false;
        }

        // Ensure it's valid
        ValidatePackage(definition);

        if (definition.Id == default)
        {
            // Create dto from definition
            var dto = new CreatedPackageSchemaDto
            {
                Name = definition.Name,
                Value = _xmlParser.ToXml(definition).ToString(),
                UpdateDate = DateTime.Now,
                PackageId = Guid.NewGuid(),
            };

            // Set the ids, we have to save in database first to get the Id
            _umbracoDatabase!.Insert(dto);
            definition.Id = dto.Id;
        }

        // Save snapshot locally, we do this to the updated packagePath
        ExportPackage(definition);

        // Create dto from definition
        var updatedDto = new CreatedPackageSchemaDto
        {
            Name = definition.Name,
            Value = _xmlParser.ToXml(definition).ToString(),
            Id = definition.Id,
            PackageId = definition.PackageId,
            UpdateDate = DateTime.Now,
        };
        _umbracoDatabase?.Update(updatedDto);

        return true;
    }

    public string ExportPackage(PackageDefinition definition)
    {
        // Ensure it's valid
        ValidatePackage(definition);

        // Create a folder for building this package
        var temporaryPath =
            _hostingEnvironment.MapPathContentRoot(Path.Combine(_tempFolderPath, Guid.NewGuid().ToString()));
        Directory.CreateDirectory(temporaryPath);

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
            PackageStaticFiles(definition.Scripts, root, "Scripts", "Script", _fileSystems.ScriptsFileSystem!);
            PackageStaticFiles(definition.PartialViews, root, "PartialViews", "View", _fileSystems.PartialViewsFileSystem!);
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

            // Clean existing files
            foreach (var packagePath in new[] { definition.PackagePath, finalPackagePath })
            {
                if (File.Exists(packagePath))
                {
                    File.Delete(packagePath);
                }
            }

            // Move to final package path
            File.Move(tempPackagePath, finalPackagePath);

            definition.PackagePath = finalPackagePath;

            return finalPackagePath;
        }
        finally
        {
            // Clean up
            Directory.Delete(temporaryPath, true);
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

    private XDocument CreateCompiledPackageXml(out XElement root)
    {
        root = new XElement("umbPackage");
        var compiledPackageXml = new XDocument(root);
        return compiledPackageXml;
    }

    private void ValidatePackage(PackageDefinition definition)
    {
        // Ensure it's valid
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

            ILanguage? lang = _localizationService.GetLanguageById(outInt);
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

            IDictionaryItem? di = _localizationService.GetDictionaryItemById(outInt);

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
            if (macroXml is null)
            {
                continue;
            }

            macros.Add(macroXml);
            packagedMacros.Add(macro!);
        }

        root.Add(macros);

        // Get the partial views for macros and package those (exclude views outside of the default directory, e.g. App_Plugins\*\Views)
        IEnumerable<string> views = packagedMacros
            .Where(x => x.MacroSource.StartsWith(Constants.SystemDirectories.MacroPartials))
            .Select(x =>
                x.MacroSource[Constants.SystemDirectories.MacroPartials.Length..].Replace('/', '\\'));
        PackageStaticFiles(views, root, "MacroPartialViews", "View", _fileSystems.MacroPartialsFileSystem!);
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
            if (xml != null)
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
        IFileSystem fileSystem)
    {
        var scriptsXml = new XElement(containerName);
        foreach (var file in filePaths)
        {
            if (file.IsNullOrWhiteSpace())
            {
                continue;
            }

            if (!fileSystem.FileExists(file))
            {
                throw new InvalidOperationException("No file found with path " + file);
            }

            using Stream stream = fileSystem.OpenFile(file);

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
        if (string.IsNullOrEmpty(definition.ContentNodeId) == false && int.TryParse(
                definition.ContentNodeId,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var contentNodeId))
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
            Stream mediaStream = _mediaFileManager.GetFile(media, out var mediaFilePath);
            if (mediaFilePath is not null)
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

    /// <summary>
    ///     Gets a macros xml node
    /// </summary>
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
}

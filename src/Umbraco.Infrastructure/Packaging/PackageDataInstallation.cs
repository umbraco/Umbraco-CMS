using System.Globalization;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Packaging
{
    public class PackageDataInstallation
    {
        private readonly IDataValueEditorFactory _dataValueEditorFactory;
        private readonly ILogger<PackageDataInstallation> _logger;
        private readonly IFileService _fileService;
        private readonly IMacroService _macroService;
        private readonly ILocalizationService _localizationService;
        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IScopeProvider _scopeProvider;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IConfigurationEditorJsonSerializer _serializer;
        private readonly IMediaService _mediaService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IEntityService _entityService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IContentService _contentService;

        public PackageDataInstallation(
            IDataValueEditorFactory dataValueEditorFactory,
            ILogger<PackageDataInstallation> logger,
            IFileService fileService,
            IMacroService macroService,
            ILocalizationService localizationService,
            IDataTypeService dataTypeService,
            IEntityService entityService,
            IContentTypeService contentTypeService,
            IContentService contentService,
            PropertyEditorCollection propertyEditors,
            IScopeProvider scopeProvider,
            IShortStringHelper shortStringHelper,
            IConfigurationEditorJsonSerializer serializer,
            IMediaService mediaService,
            IMediaTypeService mediaTypeService)
        {
            _dataValueEditorFactory = dataValueEditorFactory;
            _logger = logger;
            _fileService = fileService;
            _macroService = macroService;
            _localizationService = localizationService;
            _dataTypeService = dataTypeService;
            _entityService = entityService;
            _contentTypeService = contentTypeService;
            _contentService = contentService;
            _propertyEditors = propertyEditors;
            _scopeProvider = scopeProvider;
            _shortStringHelper = shortStringHelper;
            _serializer = serializer;
            _mediaService = mediaService;
            _mediaTypeService = mediaTypeService;
        }

        // Also remove factory service registration when this constructor is removed
        [Obsolete("Use the constructor with Infrastructure.IScopeProvider and without global settings and hosting environment instead.")]
        public PackageDataInstallation(
            IDataValueEditorFactory dataValueEditorFactory,
            ILogger<PackageDataInstallation> logger,
            IFileService fileService,
            IMacroService macroService,
            ILocalizationService localizationService,
            IDataTypeService dataTypeService,
            IEntityService entityService,
            IContentTypeService contentTypeService,
            IContentService contentService,
            PropertyEditorCollection propertyEditors,
            Core.Scoping.IScopeProvider scopeProvider,
            IShortStringHelper shortStringHelper,
            IOptions<GlobalSettings> globalSettings,
            IConfigurationEditorJsonSerializer serializer,
            IMediaService mediaService,
            IMediaTypeService mediaTypeService,
            IHostingEnvironment hostingEnvironment)
            : this(
                  dataValueEditorFactory,
                  logger,
                  fileService,
                  macroService,
                  localizationService,
                  dataTypeService,
                  entityService,
                  contentTypeService,
                  contentService,
                  propertyEditors,
                  (Umbraco.Cms.Infrastructure.Scoping.IScopeProvider) scopeProvider,
                  shortStringHelper,
                  serializer,
                  mediaService,
                  mediaTypeService)
        { }

        #region Install/Uninstall

            public InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var installationSummary = new InstallationSummary(compiledPackage.Name)
                {
                    Warnings = compiledPackage.Warnings,
                    DataTypesInstalled =
                        ImportDataTypes(compiledPackage.DataTypes.ToList(), userId,
                            out IEnumerable<EntityContainer> dataTypeEntityContainersInstalled),
                    LanguagesInstalled = ImportLanguages(compiledPackage.Languages, userId),
                    DictionaryItemsInstalled = ImportDictionaryItems(compiledPackage.DictionaryItems, userId),
                    MacrosInstalled = ImportMacros(compiledPackage.Macros, userId),
                    MacroPartialViewsInstalled = ImportMacroPartialViews(compiledPackage.MacroPartialViews, userId),
                    TemplatesInstalled = ImportTemplates(compiledPackage.Templates.ToList(), userId),
                    DocumentTypesInstalled =
                        ImportDocumentTypes(compiledPackage.DocumentTypes, userId,
                            out IEnumerable<EntityContainer> documentTypeEntityContainersInstalled),
                    MediaTypesInstalled =
                        ImportMediaTypes(compiledPackage.MediaTypes, userId,
                            out IEnumerable<EntityContainer> mediaTypeEntityContainersInstalled),
                    StylesheetsInstalled = ImportStylesheets(compiledPackage.Stylesheets, userId),
                    ScriptsInstalled = ImportScripts(compiledPackage.Scripts, userId),
                    PartialViewsInstalled = ImportPartialViews(compiledPackage.PartialViews, userId)
                };

                var entityContainersInstalled = new List<EntityContainer>();
                entityContainersInstalled.AddRange(dataTypeEntityContainersInstalled);
                entityContainersInstalled.AddRange(documentTypeEntityContainersInstalled);
                entityContainersInstalled.AddRange(mediaTypeEntityContainersInstalled);
                installationSummary.EntityContainersInstalled = entityContainersInstalled;

                // We need a reference to the imported doc types to continue
                var importedDocTypes = installationSummary.DocumentTypesInstalled.ToDictionary(x => x.Alias, x => x);
                var importedMediaTypes = installationSummary.MediaTypesInstalled.ToDictionary(x => x.Alias, x => x);

                installationSummary.ContentInstalled = ImportContentBase(compiledPackage.Documents, importedDocTypes,
                    userId, _contentTypeService, _contentService);
                installationSummary.MediaInstalled = ImportContentBase(compiledPackage.Media, importedMediaTypes,
                    userId, _mediaTypeService, _mediaService);

                scope.Complete();

                return installationSummary;
            }
        }

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="docTypeElements">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <returns>An enumerable list of generated ContentTypes</returns>
        public IReadOnlyList<IMediaType> ImportMediaTypes(IEnumerable<XElement> docTypeElements, int userId)
            => ImportMediaTypes(docTypeElements, userId, out _);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="docTypeElements">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="entityContainersInstalled">Collection of entity containers installed by the package to be populated with those created in installing data types.</param>
        /// <returns>An enumerable list of generated ContentTypes</returns>
        public IReadOnlyList<IMediaType> ImportMediaTypes(IEnumerable<XElement> docTypeElements, int userId,
            out IEnumerable<EntityContainer> entityContainersInstalled)
            => ImportDocumentTypes(docTypeElements.ToList(), true, userId, _mediaTypeService,
                out entityContainersInstalled);

        #endregion

        #region Content

        public IReadOnlyList<TContentBase> ImportContentBase<TContentBase, TContentTypeComposition>(
            IEnumerable<CompiledPackageContentBase> docs,
            IDictionary<string, TContentTypeComposition> importedDocumentTypes,
            int userId,
            IContentTypeBaseService<TContentTypeComposition> typeService,
            IContentServiceBase<TContentBase> service)
            where TContentBase : class, IContentBase
            where TContentTypeComposition : IContentTypeComposition
            => docs.SelectMany(x =>
                ImportContentBase(x.XmlData.Elements().Where(doc => (string?)doc.Attribute("isDoc") == string.Empty), -1,
                    importedDocumentTypes, userId, typeService, service)).ToList();

        /// <summary>
        /// Imports and saves package xml as <see cref="IContent"/>
        /// </summary>
        /// <param name="roots">The root contents to import from</param>
        /// <param name="typeService">The content type base service</param>
        /// <param name="parentId">Optional parent Id for the content being imported</param>
        /// <param name="importedDocumentTypes">A dictionary of already imported document types (basically used as a cache)</param>
        /// <param name="userId">Optional Id of the user performing the import</param>
        /// <param name="service">The content service base</param>
        /// <returns>An enumerable list of generated content</returns>
        public IEnumerable<TContentBase> ImportContentBase<TContentBase, TContentTypeComposition>(
            IEnumerable<XElement> roots,
            int parentId,
            IDictionary<string, TContentTypeComposition> importedDocumentTypes,
            int userId,
            IContentTypeBaseService<TContentTypeComposition> typeService,
            IContentServiceBase<TContentBase> service)
            where TContentBase : class, IContentBase
            where TContentTypeComposition : IContentTypeComposition
        {
            var contents = ParseContentBaseRootXml(roots, parentId, importedDocumentTypes, typeService, service)
                .ToList();
            if (contents.Any())
            {
                service.Save(contents, userId);
            }

            return contents;

            //var attribute = element.Attribute("isDoc");
            //if (attribute != null)
            //{
            //    //This is a single doc import
            //    var elements = new List<XElement> { element };
            //    var contents = ParseContentBaseRootXml(elements, parentId, importedDocumentTypes).ToList();
            //    if (contents.Any())
            //        _contentService.Save(contents, userId);

            //    return contents;
            //}

            //throw new ArgumentException(
            //    "The passed in XElement is not valid! It does not contain a root element called " +
            //    "'DocumentSet' (for structured imports) nor is the first element a Document (for single document import).");
        }

        private IEnumerable<TContentBase> ParseContentBaseRootXml<TContentBase, TContentTypeComposition>(
            IEnumerable<XElement> roots,
            int parentId,
            IDictionary<string, TContentTypeComposition> importedContentTypes,
            IContentTypeBaseService<TContentTypeComposition> typeService,
            IContentServiceBase<TContentBase> service)
            where TContentBase : class, IContentBase
            where TContentTypeComposition : IContentTypeComposition
        {
            var contents = new List<TContentBase>();
            foreach (XElement root in roots)
            {
                var contentTypeAlias = root.Name.LocalName;

                if (!importedContentTypes.ContainsKey(contentTypeAlias))
                {
                    TContentTypeComposition contentType = FindContentTypeByAlias(contentTypeAlias, typeService);
                    if (contentType == null)
                    {
                        throw new InvalidOperationException(
                            "Could not find content type with alias " + contentTypeAlias);
                    }

                    importedContentTypes.Add(contentTypeAlias, contentType);
                }

                if (TryCreateContentFromXml(root, importedContentTypes[contentTypeAlias], null, parentId, service, out TContentBase content))
                {
                    contents.Add(content);
                }

                var children = root.Elements().Where(doc => (string?)doc.Attribute("isDoc") == string.Empty).ToList();
                if (children.Count > 0)
                {
                    contents.AddRange(
                        CreateContentFromXml(children, content, importedContentTypes, typeService, service)
                            .WhereNotNull());
                }
            }

            return contents;
        }

        private IEnumerable<TContentBase> CreateContentFromXml<TContentBase, TContentTypeComposition>(
            IEnumerable<XElement> children,
            TContentBase parent,
            IDictionary<string, TContentTypeComposition> importedContentTypes,
            IContentTypeBaseService<TContentTypeComposition> typeService,
            IContentServiceBase<TContentBase> service)
            where TContentBase : class, IContentBase
            where TContentTypeComposition : IContentTypeComposition
        {
            var list = new List<TContentBase>();

            foreach (XElement child in children)
            {
                string contentTypeAlias = child.Name.LocalName;
                if (importedContentTypes.ContainsKey(contentTypeAlias) == false)
                {
                    TContentTypeComposition contentType = FindContentTypeByAlias(contentTypeAlias, typeService);

                    importedContentTypes.Add(contentTypeAlias, contentType);
                }

                // Create and add the child to the list
                if (TryCreateContentFromXml(child, importedContentTypes[contentTypeAlias], parent, default, service,
                        out TContentBase content))
                {
                    list.Add(content);
                }

                // Recursive call
                var grandChildren = child.Elements().Where(x => (string?)x.Attribute("isDoc") == string.Empty).ToList();
                if (grandChildren.Any())
                {
                    list.AddRange(CreateContentFromXml(grandChildren, content, importedContentTypes, typeService,
                        service));
                }
            }

            return list;
        }

        private bool TryCreateContentFromXml<TContentBase, TContentTypeComposition>(
            XElement element,
            TContentTypeComposition contentType,
            TContentBase? parent,
            int parentId,
            IContentServiceBase<TContentBase> service,
            out TContentBase output)
            where TContentBase : class?, IContentBase
            where TContentTypeComposition : IContentTypeComposition
        {
            Guid key = element.RequiredAttributeValue<Guid>("key");

            // we need to check if the content already exists and if so we ignore the installation for this item
            TContentBase? value = service.GetById(key);
            if (value != null)
            {
                output = value;
                return false;
            }

            var level = element.Attribute("level")?.Value ?? string.Empty;
            var sortOrder = element.Attribute("sortOrder")?.Value ?? string.Empty;
            var nodeName = element.Attribute("nodeName")?.Value ?? string.Empty;
            var templateId = element.AttributeValue<int?>("template");

            IEnumerable<XElement>? properties = from property in element.Elements()
                where property.Attribute("isDoc") == null
                select property;

            //TODO: This will almost never work, we can't reference a template by an INT Id within a package manifest, we need to change the
            // packager to package templates by UDI and resolve by the same, in 98% of cases, this isn't going to work, or it will resolve the wrong template.
            ITemplate? template = templateId.HasValue ? _fileService.GetTemplate(templateId.Value) : null;

            //now double check this is correct since its an INT it could very well be pointing to an invalid template :/
            if (template != null && contentType is IContentType contentTypex)
            {
                if (!contentTypex.IsAllowedTemplate(template.Alias))
                {
                    //well this is awkward, we'll set the template to null and it will be wired up to the default template
                    // when it's persisted in the document repository
                    template = null;
                }
            }

            TContentBase? content = CreateContent(
                nodeName,
                parent,
                parentId,
                contentType,
                key,
                int.Parse(level, CultureInfo.InvariantCulture),
                int.Parse(sortOrder, CultureInfo.InvariantCulture),
                template?.Id);

            if (content is null)
            {
                throw new InvalidOperationException("Cloud not create content");
            }

            // Handle culture specific node names
            const string nodeNamePrefix = "nodeName-";
            // Get the installed culture iso names, we create a localized content node with a culture that does not exist in the project
            // We have to use Invariant comparisons, because when we get them from ContentBase in EntityXmlSerializer they're all lowercase.
            var installedLanguages = _localizationService.GetAllLanguages().Select(l => l.IsoCode).ToArray();
            foreach (XAttribute localizedNodeName in element.Attributes()
                         .Where(a => a.Name.LocalName.InvariantStartsWith(nodeNamePrefix)))
            {
                var newCulture = localizedNodeName.Name.LocalName.Substring(nodeNamePrefix.Length);
                // Skip the culture if it does not exist in the current project
                if (installedLanguages.InvariantContains(newCulture))
                {
                    content.SetCultureName(localizedNodeName.Value, newCulture);
                }
            }

            //Here we make sure that we take composition properties in account as well
            //otherwise we would skip them and end up losing content
            Dictionary<string, IPropertyType> propTypes = contentType.CompositionPropertyTypes.Any()
                ? contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x)
                : contentType.PropertyTypes.ToDictionary(x => x.Alias, x => x);

            var foundLanguages = new HashSet<string?>();
            foreach (XElement property in properties)
            {
                string propertyTypeAlias = property.Name.LocalName;
                if (content.HasProperty(propertyTypeAlias))
                {
                    var propertyValue = property.Value;

                    // Handle properties language attributes
                    var propertyLang = property.Attribute(XName.Get("lang"))?.Value ?? null;
                    foundLanguages.Add(propertyLang);
                    if (propTypes.TryGetValue(propertyTypeAlias, out _))
                    {
                        // set property value
                        // Skip unsupported language variation, otherwise we'll get a "not supported error"
                        // We allow null, because that's invariant
                        if ( propertyLang is null || installedLanguages.InvariantContains(propertyLang))
                        {
                            content.SetValue(propertyTypeAlias, propertyValue, propertyLang);
                        }
                    }
                }
            }

            foreach (var propertyLang in foundLanguages)
            {
                if (string.IsNullOrEmpty(content.GetCultureName(propertyLang)) && propertyLang is not null &&
                    installedLanguages.InvariantContains(propertyLang))
                {
                    content.SetCultureName(nodeName, propertyLang);
                }
            }

            output = content;
            return true;
        }

        private TContentBase? CreateContent<TContentBase, TContentTypeComposition>(string name, TContentBase? parent,
            int parentId, TContentTypeComposition contentType, Guid key, int level, int sortOrder, int? templateId)
            where TContentBase : class?, IContentBase
            where TContentTypeComposition : IContentTypeComposition
        {
            switch (contentType)
            {
                case IContentType c:
                    if (parent is null)
                    {
                        return new Content(name, parentId, c)
                        {
                            Key = key, Level = level, SortOrder = sortOrder, TemplateId = templateId,
                        } as TContentBase;
                    }
                    else
                    {
                        return new Content(name, (IContent)parent, c)
                        {
                            Key = key, Level = level, SortOrder = sortOrder, TemplateId = templateId,
                        } as TContentBase;
                    }

                case IMediaType m:
                    if (parent is null)
                    {
                        return new Core.Models.Media(name, parentId, m)
                        {
                            Key = key, Level = level, SortOrder = sortOrder,
                        } as TContentBase;
                    }
                    else
                    {
                        return new Core.Models.Media(name, (IMedia)parent, m)
                        {
                            Key = key, Level = level, SortOrder = sortOrder,
                        } as TContentBase;
                    }

                default:
                    throw new NotSupportedException($"Type {typeof(TContentTypeComposition)} is not supported");
            }
        }

        #endregion

        #region DocumentTypes

        public IReadOnlyList<IContentType> ImportDocumentType(XElement docTypeElement, int userId)
            => ImportDocumentTypes(new[] {docTypeElement}, userId, out _);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="docTypeElements">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <returns>An enumerable list of generated ContentTypes</returns>
        public IReadOnlyList<IContentType> ImportDocumentTypes(IEnumerable<XElement> docTypeElements, int userId)
            => ImportDocumentTypes(docTypeElements.ToList(), true, userId, _contentTypeService, out _);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="docTypeElements">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="entityContainersInstalled">Collection of entity containers installed by the package to be populated with those created in installing data types.</param>
        /// <returns>An enumerable list of generated ContentTypes</returns>
        public IReadOnlyList<IContentType> ImportDocumentTypes(IEnumerable<XElement> docTypeElements, int userId,
            out IEnumerable<EntityContainer> entityContainersInstalled)
            => ImportDocumentTypes(docTypeElements.ToList(), true, userId, _contentTypeService,
                out entityContainersInstalled);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="unsortedDocumentTypes">Xml to import</param>
        /// <param name="importStructure">Boolean indicating whether or not to import the </param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="service">The content type service.</param>
        /// <returns>An enumerable list of generated ContentTypes</returns>
        public IReadOnlyList<T> ImportDocumentTypes<T>(IReadOnlyCollection<XElement> unsortedDocumentTypes, bool importStructure, int userId, IContentTypeBaseService<T> service)
            where T : class, IContentTypeComposition
            => ImportDocumentTypes(unsortedDocumentTypes, importStructure, userId, service);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="unsortedDocumentTypes">Xml to import</param>
        /// <param name="importStructure">Boolean indicating whether or not to import the </param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="service">The content type service</param>
        /// <param name="entityContainersInstalled">Collection of entity containers installed by the package to be populated with those created in installing data types.</param>
        /// <returns>An enumerable list of generated ContentTypes</returns>
        public IReadOnlyList<T> ImportDocumentTypes<T>(
            IReadOnlyCollection<XElement> unsortedDocumentTypes,
            bool importStructure, int userId,
            IContentTypeBaseService<T> service,
            out IEnumerable<EntityContainer> entityContainersInstalled)
            where T : class, IContentTypeComposition
        {
            var importedContentTypes = new Dictionary<string, T>();

            //When you are importing a single doc type we have to assume that the dependencies are already there.
            //Otherwise something like uSync won't work.
            var graph = new TopoGraph<string, TopoGraph.Node<string, XElement>>(x => x.Key, x => x.Dependencies);
            var isSingleDocTypeImport = unsortedDocumentTypes.Count == 1;

            Dictionary<string, int> importedFolders =
                CreateContentTypeFolderStructure(unsortedDocumentTypes, out entityContainersInstalled);

            if (isSingleDocTypeImport == false)
            {
                //NOTE Here we sort the doctype XElements based on dependencies
                //before creating the doc types - this should also allow for a better structure/inheritance support.
                foreach (XElement documentType in unsortedDocumentTypes)
                {
                    XElement elementCopy = documentType;
                    XElement? infoElement = elementCopy.Element("Info");
                    var dependencies = new HashSet<string>();

                    //Add the Master as a dependency
                    if (string.IsNullOrEmpty((string?)infoElement?.Element("Master")) == false)
                    {
                        dependencies.Add(infoElement.Element("Master")?.Value!);
                    }

                    //Add compositions as dependencies
                    XElement? compositionsElement = infoElement?.Element("Compositions");
                    if (compositionsElement != null && compositionsElement.HasElements)
                    {
                        IEnumerable<XElement>? compositions = compositionsElement.Elements("Composition").ToArray();
                        if (compositions.Any())
                        {
                            foreach (XElement composition in compositions)
                            {
                                dependencies.Add(composition.Value);
                            }
                        }
                    }

                    graph.AddItem(TopoGraph.CreateNode(infoElement!.Element("Alias")!.Value, elementCopy,
                        dependencies.ToArray()));
                }
            }

            //Sorting the Document Types based on dependencies - if its not a single doc type import ref. #U4-5921
            List<XElement> documentTypes = isSingleDocTypeImport
                ? unsortedDocumentTypes.ToList()
                : graph.GetSortedItems().Select(x => x.Item).ToList();

            //Iterate the sorted document types and create them as IContentType objects
            foreach (XElement documentType in documentTypes)
            {
                var alias = documentType.Element("Info")?.Element("Alias")?.Value;

                if (alias is not null && importedContentTypes.ContainsKey(alias) == false)
                {
                    T? contentType = service.Get(alias);

                    importedContentTypes.Add(alias, contentType == null
                        ? CreateContentTypeFromXml(documentType, importedContentTypes, service)
                        : UpdateContentTypeFromXml(documentType, contentType, importedContentTypes, service));
                }
            }

            foreach (KeyValuePair<string, T> contentType in importedContentTypes)
            {
                T ct = contentType.Value;
                if (importedFolders.ContainsKey(ct.Alias))
                {
                    ct.ParentId = importedFolders[ct.Alias];
                }
            }

            //Save the newly created/updated IContentType objects
            var list = importedContentTypes.Select(x => x.Value).ToList();
            service.Save(list, userId);

            //Now we can finish the import by updating the 'structure',
            //which requires the doc types to be saved/available in the db
            if (importStructure)
            {
                var updatedContentTypes = new List<T>();
                //Update the structure here - we can't do it until all DocTypes have been created
                foreach (XElement documentType in documentTypes)
                {
                    var alias = documentType.Element("Info")?.Element("Alias")?.Value;
                    XElement? structureElement = documentType.Element("Structure");
                    //Ensure that we only update ContentTypes which has actual structure-elements
                    if (structureElement == null || structureElement.Elements().Any() == false || alias is null)
                    {
                        continue;
                    }

                    T updated = UpdateContentTypesStructure(importedContentTypes[alias], structureElement,
                        importedContentTypes, service);
                    updatedContentTypes.Add(updated);
                }

                //Update ContentTypes with a newly added structure/list of allowed children
                if (updatedContentTypes.Any())
                {
                    service.Save(updatedContentTypes, userId);
                }
            }

            return list;
        }

        private Dictionary<string, int> CreateContentTypeFolderStructure(IEnumerable<XElement> unsortedDocumentTypes,
            out IEnumerable<EntityContainer> entityContainersInstalled)
        {
            var importedFolders = new Dictionary<string, int>();
            var trackEntityContainersInstalled = new List<EntityContainer>();

            foreach (XElement documentType in unsortedDocumentTypes)
            {
                XAttribute? foldersAttribute = documentType.Attribute("Folders");
                XElement? infoElement = documentType.Element("Info");
                if (foldersAttribute != null && infoElement != null
                                             // don't import any folder if this is a child doc type - the parent doc type will need to
                                             // exist which contains it's folders
                                             && ((string?)infoElement.Element("Master")).IsNullOrWhiteSpace())
                {
                    var alias = documentType.Element("Info")?.Element("Alias")?.Value;
                    var folders = foldersAttribute.Value.Split(Constants.CharArrays.ForwardSlash);

                    XAttribute? folderKeysAttribute = documentType.Attribute("FolderKeys");

                    Guid[] folderKeys = Array.Empty<Guid>();
                    if (folderKeysAttribute != null)
                    {
                        folderKeys = folderKeysAttribute.Value.Split(Constants.CharArrays.ForwardSlash)
                            .Select(x => Guid.Parse(x)).ToArray();
                    }

                    var rootFolder = WebUtility.UrlDecode(folders[0]);

                    EntityContainer? current = null;
                    Guid? rootFolderKey = null;
                    if (folderKeys.Length == folders.Length && folderKeys.Length > 0)
                    {
                        rootFolderKey = folderKeys[0];
                        current = _contentTypeService.GetContainer(rootFolderKey.Value);
                    }

                    // The folder might already exist, but with a different key, so check if it exists, even if there is a key.
                    // Level 1 = root level folders, there can only be one with the same name
                    current ??= _contentTypeService.GetContainers(rootFolder, 1).FirstOrDefault();

                    if (current == null)
                    {
                        Attempt<OperationResult<OperationResultType, EntityContainer>?> tryCreateFolder =
                            _contentTypeService.CreateContainer(-1, rootFolderKey ?? Guid.NewGuid(), rootFolder);

                        if (tryCreateFolder == false)
                        {
                            _logger.LogError(tryCreateFolder.Exception, "Could not create folder: {FolderName}",
                                rootFolder);
                            throw tryCreateFolder.Exception!;
                        }

                        var rootFolderId = tryCreateFolder.Result?.Entity?.Id;
                        if (rootFolderId is not null)
                        {
                            current = _contentTypeService.GetContainer(rootFolderId.Value);
                            trackEntityContainersInstalled.Add(current!);
                        }
                    }

                    importedFolders.Add(alias!, current!.Id);

                    for (var i = 1; i < folders.Length; i++)
                    {
                        var folderName = WebUtility.UrlDecode(folders[i]);
                        Guid? folderKey = folderKeys.Length == folders.Length ? folderKeys[i] : null;
                        current = CreateContentTypeChildFolder(folderName, folderKey ?? Guid.NewGuid(), current);
                        trackEntityContainersInstalled.Add(current!);
                        importedFolders[alias!] = current!.Id;
                    }
                }
            }

            entityContainersInstalled = trackEntityContainersInstalled;
            return importedFolders;
        }

        private EntityContainer? CreateContentTypeChildFolder(string folderName, Guid folderKey, IUmbracoEntity current)
        {
            IEntitySlim[] children = _entityService.GetChildren(current.Id).ToArray();
            var found = children.Any(x => x.Name.InvariantEquals(folderName) || x.Key.Equals(folderKey));
            if (found)
            {
                var containerId = children.Single(x => x.Name.InvariantEquals(folderName)).Id;
                return _contentTypeService.GetContainer(containerId);
            }

            Attempt<OperationResult<OperationResultType, EntityContainer>?> tryCreateFolder = _contentTypeService.CreateContainer(current.Id, folderKey, folderName);
            if (tryCreateFolder == false)
            {
                _logger.LogError(tryCreateFolder.Exception, "Could not create folder: {FolderName}", folderName);
                throw tryCreateFolder.Exception!;
            }

            return _contentTypeService.GetContainer(tryCreateFolder.Result!.Entity!.Id);
        }

        private T CreateContentTypeFromXml<T>(
            XElement documentType,
            IReadOnlyDictionary<string, T> importedContentTypes,
            IContentTypeBaseService<T> service)
            where T : class, IContentTypeComposition
        {
            var key = Guid.Parse(documentType.Element("Info")!.Element("Key")!.Value);

            XElement infoElement = documentType.Element("Info")!;

            //Name of the master corresponds to the parent
            XElement? masterElement = infoElement.Element("Master");

            T? parent = default;
            if (masterElement != null)
            {
                var masterAlias = masterElement.Value;
                parent = importedContentTypes.ContainsKey(masterAlias)
                    ? importedContentTypes[masterAlias]
                    : service.Get(masterAlias);
            }

            var alias = infoElement?.Element("Alias")?.Value;
            T? contentType = CreateContentType(key, parent, -1, alias!);

            if (parent != null)
            {
                contentType?.AddContentType(parent);
            }

            return UpdateContentTypeFromXml<T>(documentType, contentType, importedContentTypes, service);
        }

        private T? CreateContentType<T>(Guid key, T? parent, int parentId, string alias)
            where T : class, IContentTypeComposition
        {
            if (typeof(T) == typeof(IContentType))
            {
                if (parent is null)
                {
                    return new ContentType(_shortStringHelper, parentId) {Alias = alias, Key = key} as T;
                }
                else
                {
                    return new ContentType(_shortStringHelper, (IContentType)parent, alias) {Key = key} as T;
                }
            }

            if (typeof(T) == typeof(IMediaType))
            {
                if (parent is null)
                {
                    return new MediaType(_shortStringHelper, parentId) {Alias = alias, Key = key} as T;
                }
                else
                {
                    return new MediaType(_shortStringHelper, (IMediaType)parent, alias) {Key = key} as T;
                }
            }

            throw new NotSupportedException($"Type {typeof(T)} is not supported");
        }

        private T UpdateContentTypeFromXml<T>(
            XElement documentType,
            T? contentType,
            IReadOnlyDictionary<string, T> importedContentTypes,
            IContentTypeBaseService<T> service)
            where T : IContentTypeComposition
        {
            var key = Guid.Parse(documentType.Element("Info")!.Element("Key")!.Value);

            XElement? infoElement = documentType.Element("Info");
            XElement? defaultTemplateElement = infoElement?.Element("DefaultTemplate");

            if (contentType is null)
            {
                throw new InvalidOperationException("Content type was null");
            }
            contentType.Key = key;
            contentType.Name = infoElement!.Element("Name")!.Value;
            if (infoElement.Element("Key") != null)
            {
                contentType.Key = new Guid(infoElement.Element("Key")!.Value);
            }

            contentType.Icon = infoElement.Element("Icon")?.Value;
            contentType.Thumbnail = infoElement.Element("Thumbnail")?.Value;
            contentType.Description = infoElement.Element("Description")?.Value;

            //NOTE AllowAtRoot, IsListView, IsElement and Variations are new properties in the package xml so we need to verify it exists before using it.
            XElement? allowAtRoot = infoElement.Element("AllowAtRoot");
            if (allowAtRoot != null)
            {
                contentType.AllowedAsRoot = allowAtRoot.Value.InvariantEquals("true");
            }

            XElement? isListView = infoElement.Element("IsListView");
            if (isListView != null)
            {
                contentType.IsContainer = isListView.Value.InvariantEquals("true");
            }

            XElement? isElement = infoElement.Element("IsElement");
            if (isElement != null)
            {
                contentType.IsElement = isElement.Value.InvariantEquals("true");
            }

            XElement? variationsElement = infoElement.Element("Variations");
            if (variationsElement != null)
            {
                contentType.Variations =
                    (ContentVariation)Enum.Parse(typeof(ContentVariation), variationsElement.Value);
            }

            //Name of the master corresponds to the parent and we need to ensure that the Parent Id is set
            XElement? masterElement = infoElement.Element("Master");
            if (masterElement != null)
            {
                var masterAlias = masterElement.Value;
                T? parent = importedContentTypes.ContainsKey(masterAlias)
                    ? importedContentTypes[masterAlias]
                    : service.Get(masterAlias);

                contentType.SetParent(parent);
            }

            //Update Compositions on the ContentType to ensure that they are as is defined in the package xml
            XElement? compositionsElement = infoElement.Element("Compositions");
            if (compositionsElement != null && compositionsElement.HasElements)
            {
                XElement[] compositions = compositionsElement.Elements("Composition").ToArray();
                if (compositions.Any())
                {
                    foreach (XElement composition in compositions)
                    {
                        var compositionAlias = composition.Value;
                        T? compositionContentType = importedContentTypes.ContainsKey(compositionAlias)
                            ? importedContentTypes[compositionAlias]
                            : service.Get(compositionAlias);
                        contentType.AddContentType(compositionContentType);
                    }
                }
            }

            if (contentType is IContentType contentTypex)
            {
                UpdateContentTypesAllowedTemplates(contentTypex, infoElement.Element("AllowedTemplates"),
                    defaultTemplateElement);
            }

            UpdateContentTypesPropertyGroups(contentType, documentType.Element("Tabs"));
            UpdateContentTypesProperties(contentType, documentType.Element("GenericProperties"));

            if (contentType is IContentTypeWithHistoryCleanup withCleanup)
            {
                UpdateHistoryCleanupPolicy(withCleanup, documentType.Element("HistoryCleanupPolicy"));
            }

            return contentType;
        }

        private void UpdateHistoryCleanupPolicy(IContentTypeWithHistoryCleanup withCleanup, XElement? element)
        {
            if (element == null)
            {
                return;
            }

            withCleanup.HistoryCleanup ??= new Core.Models.ContentEditing.HistoryCleanup();

            if (bool.TryParse(element.Attribute("preventCleanup")?.Value, out var preventCleanup))
            {
                withCleanup.HistoryCleanup.PreventCleanup = preventCleanup;
            }

            if (int.TryParse(element.Attribute("keepAllVersionsNewerThanDays")?.Value, out var keepAll))
            {
                withCleanup.HistoryCleanup.KeepAllVersionsNewerThanDays = keepAll;
            }
            else
            {
                withCleanup.HistoryCleanup.KeepAllVersionsNewerThanDays = null;
            }

            if (int.TryParse(element.Attribute("keepLatestVersionPerDayForDays")?.Value, out var keepLatest))
            {
                withCleanup.HistoryCleanup.KeepLatestVersionPerDayForDays = keepLatest;
            }
            else
            {
                withCleanup.HistoryCleanup.KeepLatestVersionPerDayForDays = null;
            }
        }

        private void UpdateContentTypesAllowedTemplates(IContentType contentType, XElement? allowedTemplatesElement,
            XElement? defaultTemplateElement)
        {
            if (allowedTemplatesElement != null && allowedTemplatesElement.Elements("Template").Any())
            {
                var allowedTemplates = contentType.AllowedTemplates?.ToList();
                foreach (XElement templateElement in allowedTemplatesElement.Elements("Template"))
                {
                    var alias = templateElement.Value;
                    ITemplate? template = _fileService.GetTemplate(alias.ToSafeAlias(_shortStringHelper));
                    if (template != null)
                    {
                        if (allowedTemplates?.Any(x => x.Id == template.Id) ?? true)
                        {
                            continue;
                        }

                        allowedTemplates.Add(template);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Packager: Error handling allowed templates. Template with alias '{TemplateAlias}' could not be found.",
                            alias);
                    }
                }

                contentType.AllowedTemplates = allowedTemplates;
            }

            if (string.IsNullOrEmpty((string?)defaultTemplateElement) == false)
            {
                ITemplate? defaultTemplate =
                    _fileService.GetTemplate(defaultTemplateElement.Value.ToSafeAlias(_shortStringHelper));
                if (defaultTemplate != null)
                {
                    contentType.SetDefaultTemplate(defaultTemplate);
                }
                else
                {
                    _logger.LogWarning(
                        "Packager: Error handling default template. Default template with alias '{DefaultTemplateAlias}' could not be found.",
                        defaultTemplateElement.Value);
                }
            }
        }

        private void UpdateContentTypesPropertyGroups<T>(T contentType, XElement? propertyGroupsContainer)
            where T : IContentTypeComposition
        {
            if (propertyGroupsContainer == null)
            {
                return;
            }

            IEnumerable<XElement> propertyGroupElements = propertyGroupsContainer.Elements("Tab");
            foreach (XElement propertyGroupElement in propertyGroupElements)
            {
                var name = propertyGroupElement.Element("Caption")!
                    .Value; // TODO Rename to Name (same in EntityXmlSerializer)

                var alias = propertyGroupElement.Element("Alias")?.Value;
                if (string.IsNullOrEmpty(alias))
                {
                    alias = name.ToSafeAlias(_shortStringHelper, true);
                }

                contentType.AddPropertyGroup(alias, name);
                PropertyGroup propertyGroup = contentType.PropertyGroups[alias];

                if (Guid.TryParse(propertyGroupElement.Element("Key")?.Value, out Guid key))
                {
                    propertyGroup.Key = key;
                }

                if (Enum.TryParse<PropertyGroupType>(propertyGroupElement.Element("Type")?.Value, out PropertyGroupType type))
                {
                    propertyGroup.Type = type;
                }

                if (int.TryParse(propertyGroupElement.Element("SortOrder")?.Value, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out var sortOrder))
                {
                    // Override the sort order with the imported value
                    propertyGroup.SortOrder = sortOrder;
                }
            }
        }

        private void UpdateContentTypesProperties<T>(T contentType, XElement? genericPropertiesElement)
            where T : IContentTypeComposition
        {
            if (genericPropertiesElement is null)
            {
                return;
            }
            IEnumerable<XElement> properties = genericPropertiesElement.Elements("GenericProperty");
            foreach (XElement property in properties)
            {
                var dataTypeDefinitionId =
                    new Guid(property.Element("Definition")!.Value); //Unique Id for a DataTypeDefinition

                IDataType? dataTypeDefinition = _dataTypeService.GetDataType(dataTypeDefinitionId);

                //If no DataTypeDefinition with the guid from the xml wasn't found OR the ControlId on the DataTypeDefinition didn't match the DataType Id
                //We look up a DataTypeDefinition that matches


                //get the alias as a string for use below
                var propertyEditorAlias = property.Element("Type")!.Value.Trim();

                //If no DataTypeDefinition with the guid from the xml wasn't found OR the ControlId on the DataTypeDefinition didn't match the DataType Id
                //We look up a DataTypeDefinition that matches

                if (dataTypeDefinition == null)
                {
                    IDataType[]? dataTypeDefinitions = _dataTypeService.GetByEditorAlias(propertyEditorAlias).ToArray();
                    if (dataTypeDefinitions != null && dataTypeDefinitions.Any())
                    {
                        dataTypeDefinition = dataTypeDefinitions.FirstOrDefault();
                    }
                }
                else if (dataTypeDefinition.EditorAlias != propertyEditorAlias)
                {
                    IDataType[]? dataTypeDefinitions = _dataTypeService.GetByEditorAlias(propertyEditorAlias).ToArray();
                    if (dataTypeDefinitions != null && dataTypeDefinitions.Any())
                    {
                        dataTypeDefinition = dataTypeDefinitions.FirstOrDefault();
                    }
                }

                // For backwards compatibility, if no datatype with that ID can be found, we're letting this fail silently.
                // This means that the property will not be created.
                if (dataTypeDefinition == null)
                {
                    // TODO: We should expose this to the UI during install!
                    _logger.LogWarning(
                        "Packager: Error handling creation of PropertyType '{PropertyType}'. Could not find DataTypeDefintion with unique id '{DataTypeDefinitionId}' nor one referencing the DataType with a property editor alias (or legacy control id) '{PropertyEditorAlias}'. Did the package creator forget to package up custom datatypes? This property will be converted to a label/readonly editor if one exists.",
                        property.Element("Name")?.Value, dataTypeDefinitionId, property.Element("Type")?.Value.Trim());

                    //convert to a label!
                    dataTypeDefinition = _dataTypeService.GetByEditorAlias(Constants.PropertyEditors.Aliases.Label)?
                        .FirstOrDefault();
                    //if for some odd reason this isn't there then ignore
                    if (dataTypeDefinition == null)
                    {
                        continue;
                    }
                }

                var sortOrder = 0;
                XElement? sortOrderElement = property.Element("SortOrder");
                if (sortOrderElement != null)
                {
                    int.TryParse(sortOrderElement.Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out sortOrder);
                }

                var propertyType =
                    new PropertyType(_shortStringHelper, dataTypeDefinition, property.Element("Alias")!.Value)
                    {
                        Name = property.Element("Name")!.Value,
                        Description = (string?)property.Element("Description"),
                        Mandatory = property.Element("Mandatory") is not null && property.Element("Mandatory")!.Value.ToLowerInvariant().Equals("true"),
                        MandatoryMessage = property.Element("MandatoryMessage") != null
                            ? (string?)property.Element("MandatoryMessage")
                            : string.Empty,
                        ValidationRegExp = (string?)property.Element("Validation"),
                        ValidationRegExpMessage = property.Element("ValidationRegExpMessage") != null
                            ? (string?)property.Element("ValidationRegExpMessage")
                            : string.Empty,
                        SortOrder = sortOrder,
                        Variations = property.Element("Variations") != null
                            ? (ContentVariation)Enum.Parse(typeof(ContentVariation),
                                property.Element("Variations")!.Value)
                            : ContentVariation.Nothing,
                        LabelOnTop = property.Element("LabelOnTop") != null && property.Element("LabelOnTop")!.Value.ToLowerInvariant().Equals("true")
                    };

                if (property.Element("Key") != null)
                {
                    propertyType.Key = new Guid(property.Element("Key")!.Value);
                }

                XElement? propertyGroupElement = property.Element("Tab");
                if (propertyGroupElement == null || string.IsNullOrEmpty(propertyGroupElement.Value))
                {
                    contentType.AddPropertyType(propertyType);
                }
                else
                {
                    var propertyGroupName = propertyGroupElement.Value;
                    var propertyGroupAlias = propertyGroupElement.Attribute("Alias")?.Value;
                    if (string.IsNullOrEmpty(propertyGroupAlias))
                    {
                        propertyGroupAlias = propertyGroupName.ToSafeAlias(_shortStringHelper, true);
                    }

                    contentType.AddPropertyType(propertyType, propertyGroupAlias, propertyGroupName);
                }
            }
        }

        private T UpdateContentTypesStructure<T>(T contentType, XElement structureElement,
            IReadOnlyDictionary<string, T> importedContentTypes, IContentTypeBaseService<T> service)
            where T : IContentTypeComposition
        {
            var allowedChildren = contentType.AllowedContentTypes?.ToList();
            int sortOrder = allowedChildren?.Any() ?? false ? allowedChildren.Last().SortOrder : 0;
            foreach (XElement element in structureElement.Elements())
            {
                var alias = element.Value;

                T? allowedChild = importedContentTypes.ContainsKey(alias)
                    ? importedContentTypes[alias]
                    : service.Get(alias);
                if (allowedChild == null)
                {
                    _logger.LogWarning(
                        "Packager: Error handling DocumentType structure. DocumentType with alias '{DoctypeAlias}' could not be found and was not added to the structure for '{DoctypeStructureAlias}'.",
                        alias, contentType.Alias);
                    continue;
                }

                if (allowedChildren?.Any(x => x.Id.IsValueCreated && x.Id.Value == allowedChild.Id) ?? false)
                {
                    continue;
                }

                allowedChildren?.Add(new ContentTypeSort(new Lazy<int>(() => allowedChild.Id), sortOrder,
                    allowedChild.Alias));
                sortOrder++;
            }

            contentType.AllowedContentTypes = allowedChildren;
            return contentType;
        }

        /// <summary>
        /// Used during Content import to ensure that the ContentType of a content item exists
        /// </summary>
        /// <returns></returns>
        private T FindContentTypeByAlias<T>(string contentTypeAlias, IContentTypeBaseService<T> typeService)
            where T : IContentTypeComposition
        {
            T? contentType = typeService.Get(contentTypeAlias);

            if (contentType == null)
            {
                throw new Exception($"ContentType matching the passed in Alias: '{contentTypeAlias}' was null");
            }

            return contentType;
        }

        #endregion

        #region DataTypes

        /// <summary>
        /// Imports and saves package xml as <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeElements">Xml to import</param>
        /// <param name="userId">Optional id of the user</param>
        /// <returns>An enumerable list of generated DataTypeDefinitions</returns>
        public IReadOnlyList<IDataType> ImportDataTypes(IReadOnlyCollection<XElement> dataTypeElements, int userId)
            => ImportDataTypes(dataTypeElements, userId, out _);

        /// <summary>
        /// Imports and saves package xml as <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeElements">Xml to import</param>
        /// <param name="userId">Optional id of the user</param>
        /// <param name="entityContainersInstalled">Collection of entity containers installed by the package to be populated with those created in installing data types.</param>
        /// <returns>An enumerable list of generated DataTypeDefinitions</returns>
        public IReadOnlyList<IDataType> ImportDataTypes(IReadOnlyCollection<XElement> dataTypeElements, int userId,
            out IEnumerable<EntityContainer> entityContainersInstalled)
        {
            var dataTypes = new List<IDataType>();

            Dictionary<string, int> importedFolders = CreateDataTypeFolderStructure(dataTypeElements, out entityContainersInstalled);

            foreach (XElement dataTypeElement in dataTypeElements)
            {
                var dataTypeDefinitionName = dataTypeElement.AttributeValue<string>("Name");

                Guid dataTypeDefinitionId = dataTypeElement.RequiredAttributeValue<Guid>("Definition");
                XAttribute? databaseTypeAttribute = dataTypeElement.Attribute("DatabaseType");

                var parentId = -1;
                if (dataTypeDefinitionName is not null && importedFolders.ContainsKey(dataTypeDefinitionName))
                {
                    parentId = importedFolders[dataTypeDefinitionName];
                }

                IDataType? definition = _dataTypeService.GetDataType(dataTypeDefinitionId);
                //If the datatype definition doesn't already exist we create a new according to the one in the package xml
                if (definition == null)
                {
                    ValueStorageType databaseType = databaseTypeAttribute?.Value.EnumParse<ValueStorageType>(true) ??
                                                    ValueStorageType.Ntext;

                    // the Id field is actually the string property editor Alias
                    // however, the actual editor with this alias could be installed with the package, and
                    // therefore not yet part of the _propertyEditors collection, so we cannot try and get
                    // the actual editor - going with a void editor

                    var editorAlias = dataTypeElement.Attribute("Id")?.Value?.Trim();
                    if (!_propertyEditors.TryGet(editorAlias, out IDataEditor? editor))
                    {
                        editor = new VoidEditor(_dataValueEditorFactory) {Alias = editorAlias ?? string.Empty};
                    }

                    var dataType = new DataType(editor, _serializer)
                    {
                        Key = dataTypeDefinitionId,
                        Name = dataTypeDefinitionName,
                        DatabaseType = databaseType,
                        ParentId = parentId
                    };

                    var configurationAttributeValue = dataTypeElement.Attribute("Configuration")?.Value;
                    if (!string.IsNullOrWhiteSpace(configurationAttributeValue))
                    {
                        dataType.Configuration = editor.GetConfigurationEditor()
                            .FromDatabase(configurationAttributeValue, _serializer);
                    }

                    dataTypes.Add(dataType);
                }
                else
                {
                    definition.ParentId = parentId;
                    _dataTypeService.Save(definition, userId);
                }
            }

            if (dataTypes.Count > 0)
            {
                _dataTypeService.Save(dataTypes, userId);
            }

            return dataTypes;
        }

        private Dictionary<string, int> CreateDataTypeFolderStructure(IEnumerable<XElement> datatypeElements,
            out IEnumerable<EntityContainer> entityContainersInstalled)
        {
            var importedFolders = new Dictionary<string, int>();
            var trackEntityContainersInstalled = new List<EntityContainer>();
            foreach (XElement datatypeElement in datatypeElements)
            {
                XAttribute? foldersAttribute = datatypeElement.Attribute("Folders");

                if (foldersAttribute != null)
                {
                    var name = datatypeElement.Attribute("Name")?.Value;
                    var folders = foldersAttribute.Value.Split(Constants.CharArrays.ForwardSlash);
                    XAttribute? folderKeysAttribute = datatypeElement.Attribute("FolderKeys");

                    Guid[] folderKeys = Array.Empty<Guid>();
                    if (folderKeysAttribute != null)
                    {
                        folderKeys = folderKeysAttribute.Value.Split(Constants.CharArrays.ForwardSlash)
                            .Select(x => Guid.Parse(x)).ToArray();
                    }

                    var rootFolder = WebUtility.UrlDecode(folders[0]);
                    Guid rootFolderKey = folderKeys.Length > 0 ? folderKeys[0] : Guid.NewGuid();
                    //there will only be a single result by name for level 1 (root) containers
                    EntityContainer? current = _dataTypeService.GetContainers(rootFolder, 1).FirstOrDefault();

                    if (current == null)
                    {
                        Attempt<OperationResult<OperationResultType, EntityContainer>?> tryCreateFolder = _dataTypeService.CreateContainer(-1, rootFolderKey, rootFolder);
                        if (tryCreateFolder == false)
                        {
                            _logger.LogError(tryCreateFolder.Exception, "Could not create folder: {FolderName}",
                                rootFolder);
                            throw tryCreateFolder.Exception!;
                        }

                        current = _dataTypeService.GetContainer(tryCreateFolder.Result!.Entity!.Id);
                        trackEntityContainersInstalled.Add(current!);
                    }

                    importedFolders.Add(name!, current!.Id);

                    for (var i = 1; i < folders.Length; i++)
                    {
                        var folderName = WebUtility.UrlDecode(folders[i]);
                        Guid? folderKey = folderKeys.Length == folders.Length ? folderKeys[i] : null;
                        current = CreateDataTypeChildFolder(folderName, folderKey ?? Guid.NewGuid(), current);
                        trackEntityContainersInstalled.Add(current!);
                        importedFolders[name!] = current!.Id;
                    }
                }
            }

            entityContainersInstalled = trackEntityContainersInstalled;
            return importedFolders;
        }

        private EntityContainer? CreateDataTypeChildFolder(string folderName, Guid folderKey, IUmbracoEntity current)
        {
            IEntitySlim[] children = _entityService.GetChildren(current.Id).ToArray();
            var found = children.Any(x => x.Name.InvariantEquals(folderName) || x.Key.Equals(folderKey));
            if (found)
            {
                var containerId = children.Single(x => x.Name.InvariantEquals(folderName)).Id;
                return _dataTypeService.GetContainer(containerId);
            }

            Attempt<OperationResult<OperationResultType, EntityContainer>?> tryCreateFolder = _dataTypeService.CreateContainer(current.Id, folderKey, folderName);
            if (tryCreateFolder == false)
            {
                _logger.LogError(tryCreateFolder.Exception, "Could not create folder: {FolderName}", folderName);
                throw tryCreateFolder.Exception!;
            }

            return _dataTypeService.GetContainer(tryCreateFolder.Result!.Entity!.Id);
        }

        #endregion

        #region Dictionary Items

        /// <summary>
        /// Imports and saves the 'DictionaryItems' part of the package xml as a list of <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="dictionaryItemElementList">Xml to import</param>
        /// <param name="userId"></param>
        /// <returns>An enumerable list of dictionary items</returns>
        public IReadOnlyList<IDictionaryItem> ImportDictionaryItems(IEnumerable<XElement> dictionaryItemElementList,
            int userId)
        {
            var languages = _localizationService.GetAllLanguages().ToList();
            return ImportDictionaryItems(dictionaryItemElementList, languages, null, userId);
        }

        public IEnumerable<IDictionaryItem> ImportDictionaryItem(XElement dictionaryItemElement, int userId, Guid? parentId)
        {
            var languages = _localizationService.GetAllLanguages().ToList();
            return ImportDictionaryItem(dictionaryItemElement, languages, parentId, userId);
        }

        private IReadOnlyList<IDictionaryItem> ImportDictionaryItems(IEnumerable<XElement> dictionaryItemElementList,
            List<ILanguage> languages, Guid? parentId, int userId)
        {
            var items = new List<IDictionaryItem>();
            foreach (XElement dictionaryItemElement in dictionaryItemElementList)
            {
                items.AddRange(ImportDictionaryItem(dictionaryItemElement, languages, parentId, userId));
            }

            return items;
        }

        private IEnumerable<IDictionaryItem> ImportDictionaryItem(XElement dictionaryItemElement,
            List<ILanguage> languages, Guid? parentId, int userId)
        {
            var items = new List<IDictionaryItem>();

            IDictionaryItem? dictionaryItem;
            var itemName = dictionaryItemElement.Attribute("Name")?.Value;
            Guid key = dictionaryItemElement.RequiredAttributeValue<Guid>("Key");

            dictionaryItem = _localizationService.GetDictionaryItemById(key);
            if (dictionaryItem != null)
            {
                dictionaryItem = UpdateDictionaryItem(dictionaryItem, dictionaryItemElement, languages);
            }
            else
            {
                dictionaryItem = CreateNewDictionaryItem(key, itemName!, dictionaryItemElement, languages, parentId);
            }

            _localizationService.Save(dictionaryItem, userId);
            items.Add(dictionaryItem);

            items.AddRange(ImportDictionaryItems(dictionaryItemElement.Elements("DictionaryItem"), languages,
                dictionaryItem.Key, userId));
            return items;
        }

        private IDictionaryItem UpdateDictionaryItem(IDictionaryItem dictionaryItem, XElement dictionaryItemElement,
            List<ILanguage> languages)
        {
            var translations = dictionaryItem.Translations.ToList();
            foreach (XElement valueElement in dictionaryItemElement.Elements("Value")
                         .Where(v => DictionaryValueIsNew(translations, v)))
            {
                AddDictionaryTranslation(translations, valueElement, languages);
            }

            dictionaryItem.Translations = translations;
            return dictionaryItem;
        }

        private static DictionaryItem CreateNewDictionaryItem(Guid itemId, string itemName,
            XElement dictionaryItemElement, List<ILanguage> languages, Guid? parentId)
        {
            DictionaryItem dictionaryItem = parentId.HasValue
                ? new DictionaryItem(parentId.Value, itemName)
                : new DictionaryItem(itemName);
            dictionaryItem.Key = itemId;

            var translations = new List<IDictionaryTranslation>();

            foreach (XElement valueElement in dictionaryItemElement.Elements("Value"))
            {
                AddDictionaryTranslation(translations, valueElement, languages);
            }

            dictionaryItem.Translations = translations;
            return dictionaryItem;
        }

        private static bool DictionaryValueIsNew(IEnumerable<IDictionaryTranslation> translations,
            XElement valueElement)
            => translations.All(t => string.Compare(t.Language?.IsoCode,
                                         valueElement.Attribute("LanguageCultureAlias")?.Value,
                                         StringComparison.InvariantCultureIgnoreCase) !=
                                     0);

        private static void AddDictionaryTranslation(ICollection<IDictionaryTranslation> translations,
            XElement valueElement, IEnumerable<ILanguage> languages)
        {
            var languageId = valueElement.Attribute("LanguageCultureAlias")?.Value;
            ILanguage? language = languages.SingleOrDefault(l => l.IsoCode == languageId);
            if (language == null)
            {
                return;
            }

            var translation = new DictionaryTranslation(language, valueElement.Value);
            translations.Add(translation);
        }

        #endregion

        #region Languages

        /// <summary>
        /// Imports and saves the 'Languages' part of a package xml as a list of <see cref="ILanguage"/>
        /// </summary>
        /// <param name="languageElements">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation</param>
        /// <returns>An enumerable list of generated languages</returns>
        public IReadOnlyList<ILanguage> ImportLanguages(IEnumerable<XElement> languageElements, int userId)
        {
            var list = new List<ILanguage>();
            foreach (XElement languageElement in languageElements)
            {
                var isoCode = languageElement.AttributeValue<string>("CultureAlias");
                if (string.IsNullOrEmpty(isoCode))
                {
                    continue;
                }

                ILanguage? existingLanguage = _localizationService.GetLanguageByIsoCode(isoCode);
                if (existingLanguage != null)
                {
                    continue;
                }

                var cultureName = languageElement.AttributeValue<string>("FriendlyName") ?? isoCode;

                var langauge = new Language(isoCode, cultureName);
                _localizationService.Save(langauge, userId);

                list.Add(langauge);
            }

            return list;
        }

        #endregion

        #region Macros

        /// <summary>
        /// Imports and saves the 'Macros' part of a package xml as a list of <see cref="IMacro"/>
        /// </summary>
        /// <param name="macroElements">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation</param>
        /// <returns></returns>
        public IReadOnlyList<IMacro> ImportMacros(
            IEnumerable<XElement> macroElements,
            int userId)
        {
            var macros = macroElements.Select(ParseMacroElement).ToList();

            foreach (IMacro macro in macros)
            {
                _macroService.Save(macro, userId);
            }

            return macros;
        }

        public IReadOnlyList<IPartialView> ImportMacroPartialViews(IEnumerable<XElement> macroPartialViewsElements,
            int userId)
        {
            var result = new List<IPartialView>();

            foreach (XElement macroPartialViewXml in macroPartialViewsElements)
            {
                var path = macroPartialViewXml.AttributeValue<string>("path");
                if (path == null)
                {
                    throw new InvalidOperationException("No path attribute found");
                }

                // Remove prefix to maintain backwards compatibility
                if (path.StartsWith(Constants.SystemDirectories.MacroPartials))
                {
                    path = path.Substring(Constants.SystemDirectories.MacroPartials.Length);
                }
                else if (path.StartsWith("~"))
                {
                    _logger.LogWarning(
                        "Importing macro partial views outside of the Views/MacroPartials directory is not supported: {Path}",
                        path);
                    continue;
                }

                IPartialView? macroPartialView = _fileService.GetPartialViewMacro(path);

                // only update if it doesn't exist
                if (macroPartialView == null)
                {
                    var content = macroPartialViewXml.Value ?? string.Empty;

                    macroPartialView = new PartialView(PartialViewType.PartialViewMacro, path) {Content = content};
                    _fileService.SavePartialViewMacro(macroPartialView, userId);
                    result.Add(macroPartialView);
                }
            }

            return result;
        }

        private IMacro ParseMacroElement(XElement macroElement)
        {
            var macroKey = Guid.Parse(macroElement.Element("key")!.Value);
            var macroName = macroElement.Element("name")?.Value;
            var macroAlias = macroElement.Element("alias")!.Value;
            var macroSource = macroElement.Element("macroSource")!.Value;

            //Following xml elements are treated as nullable properties
            XElement? useInEditorElement = macroElement.Element("useInEditor");
            var useInEditor = false;
            if (useInEditorElement != null && string.IsNullOrEmpty((string)useInEditorElement) == false)
            {
                useInEditor = bool.Parse(useInEditorElement.Value);
            }

            XElement? cacheDurationElement = macroElement.Element("refreshRate");
            var cacheDuration = 0;
            if (cacheDurationElement != null && string.IsNullOrEmpty((string)cacheDurationElement) == false)
            {
                cacheDuration = int.Parse(cacheDurationElement.Value, CultureInfo.InvariantCulture);
            }

            XElement? cacheByMemberElement = macroElement.Element("cacheByMember");
            var cacheByMember = false;
            if (cacheByMemberElement != null && string.IsNullOrEmpty((string)cacheByMemberElement) == false)
            {
                cacheByMember = bool.Parse(cacheByMemberElement.Value);
            }

            XElement? cacheByPageElement = macroElement.Element("cacheByPage");
            var cacheByPage = false;
            if (cacheByPageElement != null && string.IsNullOrEmpty((string)cacheByPageElement) == false)
            {
                cacheByPage = bool.Parse(cacheByPageElement.Value);
            }

            XElement? dontRenderElement = macroElement.Element("dontRender");
            var dontRender = true;
            if (dontRenderElement != null && string.IsNullOrEmpty((string)dontRenderElement) == false)
            {
                dontRender = bool.Parse(dontRenderElement.Value);
            }

            var existingMacro = _macroService.GetById(macroKey) as Macro;
            Macro macro = existingMacro ?? new Macro(_shortStringHelper, macroAlias, macroName, macroSource,
                cacheByPage, cacheByMember, dontRender, useInEditor, cacheDuration) {Key = macroKey};

            XElement? properties = macroElement.Element("properties");
            if (properties != null)
            {
                int sortOrder = 0;
                foreach (XElement property in properties.Elements())
                {
                    Guid propertyKey = property.RequiredAttributeValue<Guid>("key");
                    var propertyName = property.Attribute("name")?.Value;
                    var propertyAlias = property.Attribute("alias")!.Value;
                    var editorAlias = property.Attribute("propertyType")!.Value;
                    XAttribute? sortOrderAttribute = property.Attribute("sortOrder");
                    if (sortOrderAttribute != null)
                    {
                        sortOrder = int.Parse(sortOrderAttribute.Value, CultureInfo.InvariantCulture);
                    }

                    if (macro.Properties.Values.Any(x =>
                            string.Equals(x.Alias, propertyAlias, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    macro.Properties.Add(new MacroProperty(propertyAlias, propertyName, sortOrder, editorAlias)
                    {
                        Key = propertyKey
                    });

                    sortOrder++;
                }
            }

            return macro;
        }

        #endregion

        public IReadOnlyList<IScript> ImportScripts(IEnumerable<XElement> scriptElements, int userId)
        {
            var result = new List<IScript>();

            foreach (XElement scriptXml in scriptElements)
            {
                var path = scriptXml.AttributeValue<string>("path");
                if (path.IsNullOrWhiteSpace())
                {
                    continue;
                }

                IScript? script = _fileService.GetScript(path!);

                // only update if it doesn't exist
                if (script == null)
                {
                    var content = scriptXml.Value;
                    if (content == null)
                    {
                        continue;
                    }

                    script = new Script(path!) {Content = content};
                    _fileService.SaveScript(script, userId);
                    result.Add(script);
                }
            }

            return result;
        }

        public IReadOnlyList<IPartialView> ImportPartialViews(IEnumerable<XElement> partialViewElements, int userId)
        {
            var result = new List<IPartialView>();

            foreach (XElement partialViewXml in partialViewElements)
            {
                var path = partialViewXml.AttributeValue<string>("path");

                if (path == null)
                {
                    throw new InvalidOperationException("No path attribute found");
                }

                IPartialView? partialView = _fileService.GetPartialView(path);

                // only update if it doesn't exist
                if (partialView == null)
                {
                    var content = partialViewXml.Value ?? string.Empty;

                    partialView = new PartialView(PartialViewType.PartialView, path) {Content = content};
                    _fileService.SavePartialView(partialView, userId);
                    result.Add(partialView);
                }
            }

            return result;
        }

        #region Stylesheets

        public IReadOnlyList<IFile> ImportStylesheets(IEnumerable<XElement> stylesheetElements, int userId)
        {
            var result = new List<IFile>();

            foreach (XElement n in stylesheetElements)
            {
                var stylesheetPath = n.Element("FileName")?.Value;
                if (stylesheetPath.IsNullOrWhiteSpace())
                {
                    continue;
                }

                IStylesheet? s = _fileService.GetStylesheet(stylesheetPath!);
                if (s == null)
                {
                    var content = n.Element("Content")?.Value;
                    if (content == null)
                    {
                        continue;
                    }

                    s = new Stylesheet(stylesheetPath!) {Content = content};
                    _fileService.SaveStylesheet(s, userId);
                }

                foreach (XElement prop in n.XPathSelectElements("Properties/Property"))
                {
                    var alias = prop.Element("Alias")!.Value;
                    IStylesheetProperty? sp = s.Properties?.SingleOrDefault(p => p != null && p.Alias == alias);
                    var name = prop.Element("Name")!.Value;
                    if (sp == null)
                    {
                        sp = new StylesheetProperty(name, "#" + name.ToSafeAlias(_shortStringHelper), string.Empty);
                        s.AddProperty(sp);
                    }
                    else
                    {
                        //sp.Text = name;
                        //Changing the name requires removing the current property and then adding another new one
                        if (sp.Name != name)
                        {
                            s.RemoveProperty(sp.Name);
                            var newProp = new StylesheetProperty(name, sp.Alias, sp.Value);
                            s.AddProperty(newProp);
                            sp = newProp;
                        }
                    }

                    sp.Alias = alias;
                    sp.Value = prop.Element("Value")!.Value;
                }

                _fileService.SaveStylesheet(s, userId);
                result.Add(s);
            }

            return result;
        }

        #endregion

        #region Templates

        public IEnumerable<ITemplate> ImportTemplate(XElement templateElement, int userId)
            => ImportTemplates(new[] {templateElement}, userId);

        /// <summary>
        /// Imports and saves package xml as <see cref="ITemplate"/>
        /// </summary>
        /// <param name="templateElements">Xml to import</param>
        /// <param name="userId">Optional user id</param>
        /// <returns>An enumerable list of generated Templates</returns>
        public IReadOnlyList<ITemplate> ImportTemplates(IReadOnlyCollection<XElement> templateElements, int userId)
        {
            var templates = new List<ITemplate>();

            var graph = new TopoGraph<string, TopoGraph.Node<string, XElement>>(x => x.Key, x => x.Dependencies);

            foreach (XElement tempElement in templateElements)
            {
                var dependencies = new List<string>();
                XElement elementCopy = tempElement;
                //Ensure that the Master of the current template is part of the import, otherwise we ignore this dependency as part of the dependency sorting.
                if (string.IsNullOrEmpty((string?)elementCopy.Element("Master")) == false &&
                    templateElements.Any(x => (string?)x.Element("Alias") == (string?)elementCopy.Element("Master")))
                {
                    dependencies.Add((string)elementCopy.Element("Master")!);
                }
                else if (string.IsNullOrEmpty((string?)elementCopy.Element("Master")) == false &&
                         templateElements.Any(x =>
                             (string?)x.Element("Alias") == (string?)elementCopy.Element("Master")) == false)
                {
                    _logger.LogInformation(
                        "Template '{TemplateAlias}' has an invalid Master '{TemplateMaster}', so the reference has been ignored.",
                        (string?)elementCopy.Element("Alias"),
                        (string?)elementCopy.Element("Master"));
                }

                graph.AddItem(TopoGraph.CreateNode((string)elementCopy.Element("Alias")!, elementCopy, dependencies));
            }

            //Sort templates by dependencies to a potential master template
            IEnumerable<TopoGraph.Node<string, XElement>> sorted = graph.GetSortedItems();
            foreach (TopoGraph.Node<string, XElement>? item in sorted)
            {
                XElement templateElement = item.Item;

                var templateName = templateElement.Element("Name")?.Value;
                var alias = templateElement.Element("Alias")!.Value;
                var design = templateElement.Element("Design")?.Value;
                XElement? masterElement = templateElement.Element("Master");

                var existingTemplate = _fileService.GetTemplate(alias) as Template;

                Template? template = existingTemplate ?? new Template(_shortStringHelper, templateName, alias);

                // For new templates, use the serialized key if avaialble.
                if (existingTemplate == null && Guid.TryParse(templateElement.Element("Key")?.Value, out Guid key))
                {
                    template.Key = key;
                }

                template.Content = design;

                if (masterElement != null && string.IsNullOrEmpty((string)masterElement) == false)
                {
                    template.MasterTemplateAlias = masterElement.Value;
                    ITemplate? masterTemplate = templates.FirstOrDefault(x => x.Alias == masterElement.Value);
                    if (masterTemplate != null)
                    {
                        template.MasterTemplateId = new Lazy<int>(() => masterTemplate.Id);
                    }
                }

                templates.Add(template);
            }

            if (templates.Any())
            {
                _fileService.SaveTemplate(templates, userId);
            }

            return templates;
        }

        #endregion
    }
}

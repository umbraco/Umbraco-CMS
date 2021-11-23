using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Collections;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Packaging
{
    internal class PackageDataInstallation
    {
        private readonly ILogger _logger;
        private readonly IFileService _fileService;
        private readonly IMacroService _macroService;
        private readonly ILocalizationService _localizationService;
        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IScopeProvider _scopeProvider;
        private readonly IEntityService _entityService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IContentService _contentService;

        public PackageDataInstallation(ILogger logger, IFileService fileService, IMacroService macroService, ILocalizationService localizationService,
            IDataTypeService dataTypeService, IEntityService entityService, IContentTypeService contentTypeService,
            IContentService contentService, PropertyEditorCollection propertyEditors, IScopeProvider scopeProvider)
        {
            _logger = logger;
            _fileService = fileService;
            _macroService = macroService;
            _localizationService = localizationService;
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;
            _scopeProvider = scopeProvider;
            _entityService = entityService;
            _contentTypeService = contentTypeService;
            _contentService = contentService;
        }

        #region Install/Uninstall

        public UninstallationSummary UninstallPackageData(PackageDefinition package, int userId)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            var removedTemplates = new List<ITemplate>();
            var removedMacros = new List<IMacro>();
            var removedContentTypes = new List<IContentType>();
            var removedDictionaryItems = new List<IDictionaryItem>();
            var removedDataTypes = new List<IDataType>();
            var removedLanguages = new List<ILanguage>();

            using (var scope = _scopeProvider.CreateScope())
            {
                //Uninstall templates
                foreach (var item in package.Templates.ToArray())
                {
                    if (int.TryParse(item, out var nId) == false) continue;
                    var found = _fileService.GetTemplate(nId);
                    if (found != null)
                    {
                        removedTemplates.Add(found);
                        _fileService.DeleteTemplate(found.Alias, userId);
                    }
                    package.Templates.Remove(nId.ToString());
                }

                //Uninstall macros
                foreach (var item in package.Macros.ToArray())
                {
                    if (int.TryParse(item, out var nId) == false) continue;
                    var macro = _macroService.GetById(nId);
                    if (macro != null)
                    {
                        removedMacros.Add(macro);
                        _macroService.Delete(macro, userId);
                    }
                    package.Macros.Remove(nId.ToString());
                }

                //Remove Document Types
                var contentTypes = new List<IContentType>();
                var contentTypeService = _contentTypeService;
                foreach (var item in package.DocumentTypes.ToArray())
                {
                    if (int.TryParse(item, out var nId) == false) continue;
                    var contentType = contentTypeService.Get(nId);
                    if (contentType == null) continue;
                    contentTypes.Add(contentType);
                    package.DocumentTypes.Remove(nId.ToString(CultureInfo.InvariantCulture));
                }

                //Order the DocumentTypes before removing them
                if (contentTypes.Any())
                {
                    // TODO: I don't think this ordering is necessary
                    var orderedTypes = (from contentType in contentTypes
                                        orderby contentType.ParentId descending, contentType.Id descending
                                        select contentType).ToList();
                    removedContentTypes.AddRange(orderedTypes);
                    contentTypeService.Delete(orderedTypes, userId);
                }

                //Remove Dictionary items
                foreach (var item in package.DictionaryItems.ToArray())
                {
                    if (int.TryParse(item, out var nId) == false) continue;
                    var di = _localizationService.GetDictionaryItemById(nId);
                    if (di != null)
                    {
                        removedDictionaryItems.Add(di);
                        _localizationService.Delete(di, userId);
                    }
                    package.DictionaryItems.Remove(nId.ToString());
                }

                //Remove Data types
                foreach (var item in package.DataTypes.ToArray())
                {
                    if (int.TryParse(item, out var nId) == false) continue;
                    var dtd = _dataTypeService.GetDataType(nId);
                    if (dtd != null)
                    {
                        removedDataTypes.Add(dtd);
                        _dataTypeService.Delete(dtd, userId);
                    }
                    package.DataTypes.Remove(nId.ToString());
                }

                //Remove Langs
                foreach (var item in package.Languages.ToArray())
                {
                    if (int.TryParse(item, out var nId) == false) continue;
                    var lang = _localizationService.GetLanguageById(nId);
                    if (lang != null)
                    {
                        removedLanguages.Add(lang);
                        _localizationService.Delete(lang, userId);
                    }
                    package.Languages.Remove(nId.ToString());
                }

                scope.Complete();
            }

            // create a summary of what was actually removed, for PackagingService.UninstalledPackage
            var summary = new UninstallationSummary
            {
                MetaData = package,
                TemplatesUninstalled = removedTemplates,
                MacrosUninstalled = removedMacros,
                DocumentTypesUninstalled = removedContentTypes,
                DictionaryItemsUninstalled = removedDictionaryItems,
                DataTypesUninstalled = removedDataTypes,
                LanguagesUninstalled = removedLanguages,

            };

            return summary;


        }

        public InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var installationSummary = new InstallationSummary
                {
                    DataTypesInstalled = ImportDataTypes(compiledPackage.DataTypes.ToList(), userId),
                    LanguagesInstalled = ImportLanguages(compiledPackage.Languages, userId),
                    DictionaryItemsInstalled = ImportDictionaryItems(compiledPackage.DictionaryItems, userId),
                    MacrosInstalled = ImportMacros(compiledPackage.Macros, userId),
                    TemplatesInstalled = ImportTemplates(compiledPackage.Templates.ToList(), userId),
                    DocumentTypesInstalled = ImportDocumentTypes(compiledPackage.DocumentTypes, userId)
                };

                //we need a reference to the imported doc types to continue
                var importedDocTypes = installationSummary.DocumentTypesInstalled.ToDictionary(x => x.Alias, x => x);

                installationSummary.StylesheetsInstalled = ImportStylesheets(compiledPackage.Stylesheets, userId);
                installationSummary.ContentInstalled = ImportContent(compiledPackage.Documents, importedDocTypes, userId);

                scope.Complete();

                return installationSummary;
            }
        }

        #endregion

        #region Content


        public IReadOnlyList<IContent> ImportContent(IEnumerable<CompiledPackageDocument> docs, IDictionary<string, IContentType> importedDocumentTypes, int userId)
        {
            return docs.SelectMany(x => ImportContent(x, -1, importedDocumentTypes, userId)).ToList();
        }

        /// <summary>
        /// Imports and saves package xml as <see cref="IContent"/>
        /// </summary>
        /// <param name="packageDocument">Xml to import</param>
        /// <param name="parentId">Optional parent Id for the content being imported</param>
        /// <param name="importedDocumentTypes">A dictionary of already imported document types (basically used as a cache)</param>
        /// <param name="userId">Optional Id of the user performing the import</param>
        /// <returns>An enumerable list of generated content</returns>
        public IEnumerable<IContent> ImportContent(CompiledPackageDocument packageDocument, int parentId, IDictionary<string, IContentType> importedDocumentTypes, int userId)
        {
            var element = packageDocument.XmlData;

            var roots = from doc in element.Elements()
                        where (string)doc.Attribute("isDoc") == ""
                        select doc;

            var contents = ParseDocumentRootXml(roots, parentId, importedDocumentTypes).ToList();
            if (contents.Any())
                _contentService.Save(contents, userId);

            return contents;

            //var attribute = element.Attribute("isDoc");
            //if (attribute != null)
            //{
            //    //This is a single doc import
            //    var elements = new List<XElement> { element };
            //    var contents = ParseDocumentRootXml(elements, parentId, importedDocumentTypes).ToList();
            //    if (contents.Any())
            //        _contentService.Save(contents, userId);

            //    return contents;
            //}

            //throw new ArgumentException(
            //    "The passed in XElement is not valid! It does not contain a root element called " +
            //    "'DocumentSet' (for structured imports) nor is the first element a Document (for single document import).");
        }

        private IEnumerable<IContent> ParseDocumentRootXml(IEnumerable<XElement> roots, int parentId, IDictionary<string, IContentType> importedContentTypes)
        {
            var contents = new List<IContent>();
            foreach (var root in roots)
            {
                var contentTypeAlias = root.Name.LocalName;

                if (!importedContentTypes.ContainsKey(contentTypeAlias))
                {
                    var contentType = FindContentTypeByAlias(contentTypeAlias);
                    importedContentTypes.Add(contentTypeAlias, contentType);
                }

                var content = CreateContentFromXml(root, importedContentTypes[contentTypeAlias], null, parentId);
                if (content == null) continue;

                contents.Add(content);

                var children = (from child in root.Elements()
                                where (string)child.Attribute("isDoc") == ""
                                select child)
                    .ToList();

                if (children.Count > 0)
                    contents.AddRange(CreateContentFromXml(children, content, importedContentTypes).WhereNotNull());
            }
            return contents;
        }

        private IEnumerable<IContent> CreateContentFromXml(IEnumerable<XElement> children, IContent parent, IDictionary<string, IContentType> importedContentTypes)
        {
            var list = new List<IContent>();
            foreach (var child in children)
            {
                string contentTypeAlias = child.Name.LocalName;

                if (importedContentTypes.ContainsKey(contentTypeAlias) == false)
                {
                    var contentType = FindContentTypeByAlias(contentTypeAlias);
                    importedContentTypes.Add(contentTypeAlias, contentType);
                }

                //Create and add the child to the list
                var content = CreateContentFromXml(child, importedContentTypes[contentTypeAlias], parent, default);
                list.Add(content);

                //Recursive call
                var child1 = child;
                var grandChildren = (from grand in child1.Elements()
                                     where (string)grand.Attribute("isDoc") == ""
                                     select grand).ToList();

                if (grandChildren.Any())
                    list.AddRange(CreateContentFromXml(grandChildren, content, importedContentTypes));
            }

            return list;
        }

        private IContent CreateContentFromXml(XElement element, IContentType contentType, IContent parent, int parentId)
        {
            var key = Guid.Empty;
            if (element.Attribute("key") != null && Guid.TryParse(element.Attribute("key").Value, out key))
            {
                //if a Key is supplied, then we need to check if the content already exists and if so we ignore the installation for this item
                if (_contentService.GetById(key) != null)
                    return null;
            }

            var id = element.Attribute("id").Value;
            var level = element.Attribute("level").Value;
            var sortOrder = element.Attribute("sortOrder").Value;
            var nodeName = element.Attribute("nodeName").Value;
            var path = element.Attribute("path").Value;
            var templateId = element.AttributeValue<int?>("template");

            var properties = from property in element.Elements()
                             where property.Attribute("isDoc") == null
                             select property;

            //TODO: This will almost never work, we can't reference a template by an INT Id within a package manifest, we need to change the
            // packager to package templates by UDI and resolve by the same, in 98% of cases, this isn't going to work, or it will resolve the wrong template.
            var template = templateId.HasValue ? _fileService.GetTemplate(templateId.Value) : null;

            //now double check this is correct since its an INT it could very well be pointing to an invalid template :/
            if (template != null)
            {
                if (!contentType.IsAllowedTemplate(template.Alias))
                {
                    //well this is awkward, we'll set the template to null and it will be wired up to the default template
                    // when it's persisted in the document repository
                    template = null;
                }
            }

            IContent content = parent == null
                ? new Content(nodeName, parentId, contentType)
                {
                    Level = int.Parse(level),
                    SortOrder = int.Parse(sortOrder),
                    TemplateId = template?.Id,
                    Key = key
                }
                : new Content(nodeName, parent, contentType)
                {
                    Level = int.Parse(level),
                    SortOrder = int.Parse(sortOrder),
                    TemplateId = template?.Id,
                    Key = key
                };

            // Handle culture specific node names
            const string nodeNamePrefix = "nodeName-";
            // Get the installed culture iso names, we create a localized content node with a culture that does not exist in the project
            // We have to use Invariant comparisons, because when we get them from ContentBase in EntityXmlSerializer they're all lowercase.
            var installedLanguages = _localizationService.GetAllLanguages().Select(l => l.IsoCode).ToArray();
            foreach (var localizedNodeName in element.Attributes().Where(a => a.Name.LocalName.InvariantStartsWith(nodeNamePrefix)))
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
            var propTypes = contentType.CompositionPropertyTypes.Any()
                ? contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x)
                : contentType.PropertyTypes.ToDictionary(x => x.Alias, x => x);

            var foundLanguages = new HashSet<string>();
            foreach (var property in properties)
            {
                string propertyTypeAlias = property.Name.LocalName;
                if (content.HasProperty(propertyTypeAlias))
                {
                    var propertyValue = property.Value;

                    // Handle properties language attributes
                    var propertyLang = property.Attribute(XName.Get("lang"))?.Value;
                    foundLanguages.Add(propertyLang);
                    if (propTypes.TryGetValue(propertyTypeAlias, out var propertyType))
                    {
                        // set property value
                        // Skip unsupported language variation, otherwise we'll get a "not supported error"
                        // We allow null, because that's invariant
                        if (installedLanguages.InvariantContains(propertyLang) || propertyLang is null)
                        {
                            content.SetValue(propertyTypeAlias, propertyValue, propertyLang);
                        }
                    }
                }
            }

            foreach (var propertyLang in foundLanguages)
            {
                if (string.IsNullOrEmpty(content.GetCultureName(propertyLang)) && installedLanguages.InvariantContains(propertyLang))
                {
                    content.SetCultureName(nodeName, propertyLang);
                }
            }

            return content;
        }

        #endregion

        #region DocumentTypes

        public IReadOnlyList<IContentType> ImportDocumentType(XElement docTypeElement, int userId)
        {
            return ImportDocumentTypes(new[] { docTypeElement }, userId);
        }

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="docTypeElements">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <returns>An enumerable list of generated ContentTypes</returns>
        public IReadOnlyList<IContentType> ImportDocumentTypes(IEnumerable<XElement> docTypeElements, int userId)
        {
            return ImportDocumentTypes(docTypeElements.ToList(), true, userId);
        }

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="unsortedDocumentTypes">Xml to import</param>
        /// <param name="importStructure">Boolean indicating whether or not to import the </param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <returns>An enumerable list of generated ContentTypes</returns>
        public IReadOnlyList<IContentType> ImportDocumentTypes(IReadOnlyCollection<XElement> unsortedDocumentTypes, bool importStructure, int userId)
        {
            var importedContentTypes = new Dictionary<string, IContentType>();

            //When you are importing a single doc type we have to assume that the dependencies are already there.
            //Otherwise something like uSync won't work.
            var graph = new TopoGraph<string, TopoGraph.Node<string, XElement>>(x => x.Key, x => x.Dependencies);
            var isSingleDocTypeImport = unsortedDocumentTypes.Count == 1;

            var importedFolders = CreateContentTypeFolderStructure(unsortedDocumentTypes);

            if (isSingleDocTypeImport == false)
            {
                //NOTE Here we sort the doctype XElements based on dependencies
                //before creating the doc types - this should also allow for a better structure/inheritance support.
                foreach (var documentType in unsortedDocumentTypes)
                {
                    var elementCopy = documentType;
                    var infoElement = elementCopy.Element("Info");
                    var dependencies = new HashSet<string>();

                    //Add the Master as a dependency
                    if (string.IsNullOrEmpty((string)infoElement.Element("Master")) == false)
                    {
                        dependencies.Add(infoElement.Element("Master").Value);
                    }

                    //Add compositions as dependencies
                    var compositionsElement = infoElement.Element("Compositions");
                    if (compositionsElement != null && compositionsElement.HasElements)
                    {
                        var compositions = compositionsElement.Elements("Composition");
                        if (compositions.Any())
                        {
                            foreach (var composition in compositions)
                            {
                                dependencies.Add(composition.Value);
                            }
                        }
                    }

                    graph.AddItem(TopoGraph.CreateNode(infoElement.Element("Alias").Value, elementCopy, dependencies.ToArray()));
                }
            }

            //Sorting the Document Types based on dependencies - if its not a single doc type import ref. #U4-5921
            var documentTypes = isSingleDocTypeImport
                ? unsortedDocumentTypes.ToList()
                : graph.GetSortedItems().Select(x => x.Item).ToList();

            //Iterate the sorted document types and create them as IContentType objects
            foreach (var documentType in documentTypes)
            {
                var alias = documentType.Element("Info").Element("Alias").Value;
                if (importedContentTypes.ContainsKey(alias) == false)
                {
                    var contentType = _contentTypeService.Get(alias);
                    importedContentTypes.Add(alias, contentType == null
                        ? CreateContentTypeFromXml(documentType, importedContentTypes)
                        : UpdateContentTypeFromXml(documentType, contentType, importedContentTypes));
                }
            }

            foreach (var contentType in importedContentTypes)
            {
                var ct = contentType.Value;
                if (importedFolders.ContainsKey(ct.Alias))
                {
                    ct.ParentId = importedFolders[ct.Alias];
                }
            }

            //Save the newly created/updated IContentType objects
            var list = importedContentTypes.Select(x => x.Value).ToList();
            _contentTypeService.Save(list, userId);

            //Now we can finish the import by updating the 'structure',
            //which requires the doc types to be saved/available in the db
            if (importStructure)
            {
                var updatedContentTypes = new List<IContentType>();
                //Update the structure here - we can't do it until all DocTypes have been created
                foreach (var documentType in documentTypes)
                {
                    var alias = documentType.Element("Info").Element("Alias").Value;
                    var structureElement = documentType.Element("Structure");
                    //Ensure that we only update ContentTypes which has actual structure-elements
                    if (structureElement == null || structureElement.Elements("DocumentType").Any() == false) continue;

                    var updated = UpdateContentTypesStructure(importedContentTypes[alias], structureElement, importedContentTypes);
                    updatedContentTypes.Add(updated);
                }
                //Update ContentTypes with a newly added structure/list of allowed children
                if (updatedContentTypes.Any())
                    _contentTypeService.Save(updatedContentTypes, userId);
            }

            return list;
        }

        private Dictionary<string, int> CreateContentTypeFolderStructure(IEnumerable<XElement> unsortedDocumentTypes)
        {
            var importedFolders = new Dictionary<string, int>();
            foreach (var documentType in unsortedDocumentTypes)
            {
                var foldersAttribute = documentType.Attribute("Folders");
                var infoElement = documentType.Element("Info");
                if (foldersAttribute != null && infoElement != null
                    //don't import any folder if this is a child doc type - the parent doc type will need to
                    //exist which contains it's folders
                    && ((string)infoElement.Element("Master")).IsNullOrWhiteSpace())
                {
                    var alias = documentType.Element("Info").Element("Alias").Value;
                    var folders = foldersAttribute.Value.Split(Constants.CharArrays.ForwardSlash);
                    var rootFolder = HttpUtility.UrlDecode(folders[0]);
                    //level 1 = root level folders, there can only be one with the same name
                    var current = _contentTypeService.GetContainers(rootFolder, 1).FirstOrDefault();

                    if (current == null)
                    {
                        var tryCreateFolder = _contentTypeService.CreateContainer(-1, rootFolder);
                        if (tryCreateFolder == false)
                        {
                            _logger.Error<PackagingService, string>(tryCreateFolder.Exception, "Could not create folder: {FolderName}", rootFolder);
                            throw tryCreateFolder.Exception;
                        }
                        var rootFolderId = tryCreateFolder.Result.Entity.Id;
                        current = _contentTypeService.GetContainer(rootFolderId);
                    }

                    importedFolders.Add(alias, current.Id);

                    for (var i = 1; i < folders.Length; i++)
                    {
                        var folderName = HttpUtility.UrlDecode(folders[i]);
                        current = CreateContentTypeChildFolder(folderName, current);
                        importedFolders[alias] = current.Id;
                    }
                }
            }

            return importedFolders;
        }

        private EntityContainer CreateContentTypeChildFolder(string folderName, IUmbracoEntity current)
        {
            var children = _entityService.GetChildren(current.Id).ToArray();
            var found = children.Any(x => x.Name.InvariantEquals(folderName));
            if (found)
            {
                var containerId = children.Single(x => x.Name.InvariantEquals(folderName)).Id;
                return _contentTypeService.GetContainer(containerId);
            }

            var tryCreateFolder = _contentTypeService.CreateContainer(current.Id, folderName);
            if (tryCreateFolder == false)
            {
                _logger.Error<PackagingService, string>(tryCreateFolder.Exception, "Could not create folder: {FolderName}", folderName);
                throw tryCreateFolder.Exception;
            }
            return _contentTypeService.GetContainer(tryCreateFolder.Result.Entity.Id);
        }

        private IContentType CreateContentTypeFromXml(XElement documentType, IReadOnlyDictionary<string, IContentType> importedContentTypes)
        {
            var infoElement = documentType.Element("Info");

            //Name of the master corresponds to the parent
            var masterElement = infoElement.Element("Master");
            IContentType parent = null;
            if (masterElement != null)
            {
                var masterAlias = masterElement.Value;
                parent = importedContentTypes.ContainsKey(masterAlias)
                             ? importedContentTypes[masterAlias]
                             : _contentTypeService.Get(masterAlias);
            }

            var alias = infoElement.Element("Alias").Value;
            var contentType = parent == null
                                  ? new ContentType(-1) { Alias = alias }
                                  : new ContentType(parent, alias);

            if (parent != null)
                contentType.AddContentType(parent);

            return UpdateContentTypeFromXml(documentType, contentType, importedContentTypes);
        }

        private IContentType UpdateContentTypeFromXml(XElement documentType, IContentType contentType, IReadOnlyDictionary<string, IContentType> importedContentTypes)
        {
            var infoElement = documentType.Element("Info");
            var defaultTemplateElement = infoElement.Element("DefaultTemplate");

            contentType.Name = infoElement.Element("Name").Value;
            if (infoElement.Element("Key") != null)
                contentType.Key = new Guid(infoElement.Element("Key").Value);
            contentType.Icon = infoElement.Element("Icon").Value;
            contentType.Thumbnail = infoElement.Element("Thumbnail").Value;
            contentType.Description = infoElement.Element("Description").Value;

            //NOTE AllowAtRoot, IsListView, IsElement and Variations are new properties in the package xml so we need to verify it exists before using it.
            var allowAtRoot = infoElement.Element("AllowAtRoot");
            if (allowAtRoot != null)
                contentType.AllowedAsRoot = allowAtRoot.Value.InvariantEquals("true");

            var isListView = infoElement.Element("IsListView");
            if (isListView != null)
                contentType.IsContainer = isListView.Value.InvariantEquals("true");

            var isElement = infoElement.Element("IsElement");
            if (isElement != null)
                contentType.IsElement = isElement.Value.InvariantEquals("true");

            var variationsElement = infoElement.Element("Variations");
            if (variationsElement != null)
                contentType.Variations = (ContentVariation)Enum.Parse(typeof(ContentVariation), variationsElement.Value);

            //Name of the master corresponds to the parent and we need to ensure that the Parent Id is set
            var masterElement = infoElement.Element("Master");
            if (masterElement != null)
            {
                var masterAlias = masterElement.Value;
                IContentType parent = importedContentTypes.ContainsKey(masterAlias)
                    ? importedContentTypes[masterAlias]
                    : _contentTypeService.Get(masterAlias);

                contentType.SetParent(parent);
            }

            //Update Compositions on the ContentType to ensure that they are as is defined in the package xml
            var compositionsElement = infoElement.Element("Compositions");
            if (compositionsElement != null && compositionsElement.HasElements)
            {
                var compositions = compositionsElement.Elements("Composition");
                if (compositions.Any())
                {
                    foreach (var composition in compositions)
                    {
                        var compositionAlias = composition.Value;
                        var compositionContentType = importedContentTypes.ContainsKey(compositionAlias)
                            ? importedContentTypes[compositionAlias]
                            : _contentTypeService.Get(compositionAlias);
                        contentType.AddContentType(compositionContentType);
                    }
                }
            }

            UpdateContentTypesAllowedTemplates(contentType, infoElement.Element("AllowedTemplates"), defaultTemplateElement);
            UpdateContentTypesPropertyGroups(contentType, documentType.Element("Tabs"));
            UpdateContentTypesProperties(contentType, documentType.Element("GenericProperties"));
            UpdateHistoryCleanupPolicy(contentType, documentType.Element("HistoryCleanupPolicy"));

            return contentType;
        }

        private void UpdateHistoryCleanupPolicy(IContentType contentType, XElement element)
        {
            if (element == null)
            {
                return;
            }

            contentType.HistoryCleanup ??= new HistoryCleanup();

            if (bool.TryParse(element.Attribute("preventCleanup")?.Value, out var preventCleanup))
            {
                contentType.HistoryCleanup.PreventCleanup = preventCleanup;
            }

            if (int.TryParse(element.Attribute("keepAllVersionsNewerThanDays")?.Value, out var keepAll))
            {
                contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = keepAll;
            }
            else
            {
                contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = null;
            }

            if (int.TryParse(element.Attribute("keepLatestVersionPerDayForDays")?.Value, out var keepLatest))
            {
                contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = keepLatest;
            }
            else
            {
                contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = null;
            }
        }

        private void UpdateContentTypesAllowedTemplates(IContentType contentType,
                                                        XElement allowedTemplatesElement, XElement defaultTemplateElement)
        {
            if (allowedTemplatesElement != null && allowedTemplatesElement.Elements("Template").Any())
            {
                var allowedTemplates = contentType.AllowedTemplates.ToList();
                foreach (var templateElement in allowedTemplatesElement.Elements("Template"))
                {
                    var alias = templateElement.Value;
                    var template = _fileService.GetTemplate(alias.ToSafeAlias());
                    if (template != null)
                    {
                        if (allowedTemplates.Any(x => x.Id == template.Id)) continue;
                        allowedTemplates.Add(template);
                    }
                    else
                    {
                        _logger.Warn<PackagingService,string>("Packager: Error handling allowed templates. Template with alias '{TemplateAlias}' could not be found.", alias);
                    }
                }

                contentType.AllowedTemplates = allowedTemplates;
            }

            if (string.IsNullOrEmpty((string)defaultTemplateElement) == false)
            {
                var defaultTemplate = _fileService.GetTemplate(defaultTemplateElement.Value.ToSafeAlias());
                if (defaultTemplate != null)
                {
                    contentType.SetDefaultTemplate(defaultTemplate);
                }
                else
                {
                    _logger.Warn<PackagingService,string>("Packager: Error handling default template. Default template with alias '{DefaultTemplateAlias}' could not be found.", defaultTemplateElement.Value);
                }
            }
        }

        private void UpdateContentTypesPropertyGroups(IContentType contentType, XElement propertyGroupsContainer)
        {
            if (propertyGroupsContainer == null)
                return;

            var propertyGroupElements = propertyGroupsContainer.Elements("Tab");
            foreach (var propertyGroupElement in propertyGroupElements)
            {
                var name = propertyGroupElement.Element("Caption").Value; // TODO Rename to Name (same in EntityXmlSerializer)

                var alias = propertyGroupElement.Element("Alias")?.Value;
                if (string.IsNullOrEmpty(alias))
                {
                    alias = name.ToSafeAlias(true);
                }

                contentType.AddPropertyGroup(alias, name);
                var propertyGroup = contentType.PropertyGroups[alias];

                if (Guid.TryParse(propertyGroupElement.Element("Key")?.Value, out var key))
                {
                    propertyGroup.Key = key;
                }

                if (Enum.TryParse<PropertyGroupType>(propertyGroupElement.Element("Type")?.Value, out var type))
                {
                    propertyGroup.Type = type;
                }

                if (int.TryParse(propertyGroupElement.Element("SortOrder")?.Value, out var sortOrder))
                {
                    // Override the sort order with the imported value
                    propertyGroup.SortOrder = sortOrder;
                }
            }
        }

        private void UpdateContentTypesProperties(IContentType contentType, XElement genericPropertiesElement)
        {
            var properties = genericPropertiesElement.Elements("GenericProperty");
            foreach (var property in properties)
            {
                var dataTypeDefinitionId = new Guid(property.Element("Definition").Value);//Unique Id for a DataTypeDefinition

                var dataTypeDefinition = _dataTypeService.GetDataType(dataTypeDefinitionId);

                //If no DataTypeDefinition with the guid from the xml wasn't found OR the ControlId on the DataTypeDefinition didn't match the DataType Id
                //We look up a DataTypeDefinition that matches


                //get the alias as a string for use below
                var propertyEditorAlias = property.Element("Type").Value.Trim();

                //If no DataTypeDefinition with the guid from the xml wasn't found OR the ControlId on the DataTypeDefinition didn't match the DataType Id
                //We look up a DataTypeDefinition that matches

                if (dataTypeDefinition == null)
                {
                    var dataTypeDefinitions = _dataTypeService.GetByEditorAlias(propertyEditorAlias);
                    if (dataTypeDefinitions != null && dataTypeDefinitions.Any())
                    {
                        dataTypeDefinition = dataTypeDefinitions.FirstOrDefault();
                    }
                }
                else if (dataTypeDefinition.EditorAlias != propertyEditorAlias)
                {
                    var dataTypeDefinitions = _dataTypeService.GetByEditorAlias(propertyEditorAlias);
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
                    _logger.Warn<PackagingService, string, Guid, string>("Packager: Error handling creation of PropertyType '{PropertyType}'. Could not find DataTypeDefintion with unique id '{DataTypeDefinitionId}' nor one referencing the DataType with a property editor alias (or legacy control id) '{PropertyEditorAlias}'. Did the package creator forget to package up custom datatypes? This property will be converted to a label/readonly editor if one exists.",
                        property.Element("Name").Value, dataTypeDefinitionId, property.Element("Type").Value.Trim());

                    //convert to a label!
                    dataTypeDefinition = _dataTypeService.GetByEditorAlias(Constants.PropertyEditors.Aliases.Label).FirstOrDefault();
                    //if for some odd reason this isn't there then ignore
                    if (dataTypeDefinition == null) continue;
                }

                var sortOrder = 0;
                var sortOrderElement = property.Element("SortOrder");
                if (sortOrderElement != null)
                    int.TryParse(sortOrderElement.Value, out sortOrder);
                var propertyType = new PropertyType(dataTypeDefinition, property.Element("Alias").Value)
                {
                    Name = property.Element("Name").Value,
                    Description = (string)property.Element("Description"),
                    Mandatory = property.Element("Mandatory") != null
                        ? property.Element("Mandatory").Value.ToLowerInvariant().Equals("true")
                        : false,
                    MandatoryMessage = property.Element("MandatoryMessage") != null
                        ? (string)property.Element("MandatoryMessage")
                        : string.Empty,

                    ValidationRegExp = (string)property.Element("Validation"),
                    ValidationRegExpMessage = property.Element("ValidationRegExpMessage") != null
                        ? (string)property.Element("ValidationRegExpMessage")
                        : string.Empty,
                    SortOrder = sortOrder,
                    Variations = property.Element("Variations") != null
                        ? (ContentVariation)Enum.Parse(typeof(ContentVariation), property.Element("Variations").Value)
                        : ContentVariation.Nothing,
                    LabelOnTop = property.Element("LabelOnTop") != null
                        ? property.Element("LabelOnTop").Value.ToLowerInvariant().Equals("true")
                        : false
                };
                if (property.Element("Key") != null)
                    propertyType.Key = new Guid(property.Element("Key").Value);

                var tabElement = property.Element("Tab");
                if (tabElement == null || string.IsNullOrEmpty(tabElement.Value))
                {
                    contentType.AddPropertyType(propertyType);
                }
                else
                {
                    var tabName = tabElement.Value;
                    var tabAlias = tabElement.Attribute("Alias")?.Value;
                    if (string.IsNullOrEmpty(tabAlias))
                    {
                        contentType.AddPropertyType(propertyType, tabName);
                    }
                    else
                    {
                        contentType.AddPropertyType(propertyType, tabAlias, tabName);
                    }
                }
            }
        }

        private IContentType UpdateContentTypesStructure(IContentType contentType, XElement structureElement, IReadOnlyDictionary<string, IContentType> importedContentTypes)
        {
            var allowedChildren = contentType.AllowedContentTypes.ToList();
            int sortOrder = allowedChildren.Any() ? allowedChildren.Last().SortOrder : 0;
            foreach (var element in structureElement.Elements("DocumentType"))
            {
                var alias = element.Value;

                var allowedChild = importedContentTypes.ContainsKey(alias) ? importedContentTypes[alias] : _contentTypeService.Get(alias);
                if (allowedChild == null)
                {
                    _logger.Warn<PackagingService, string, string>(
                        "Packager: Error handling DocumentType structure. DocumentType with alias '{DoctypeAlias}' could not be found and was not added to the structure for '{DoctypeStructureAlias}'.",
                        alias, contentType.Alias);
                    continue;
                }

                if (allowedChildren.Any(x => x.Id.IsValueCreated && x.Id.Value == allowedChild.Id)) continue;

                allowedChildren.Add(new ContentTypeSort(new Lazy<int>(() => allowedChild.Id), sortOrder, allowedChild.Alias));
                sortOrder++;
            }

            contentType.AllowedContentTypes = allowedChildren;
            return contentType;
        }

        /// <summary>
        /// Used during Content import to ensure that the ContentType of a content item exists
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns></returns>
        private IContentType FindContentTypeByAlias(string contentTypeAlias)
        {
            var contentType = _contentTypeService.Get(contentTypeAlias);

            if (contentType == null)
                throw new Exception($"ContentType matching the passed in Alias: '{contentTypeAlias}' was null");

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
        {
            var dataTypes = new List<IDataType>();

            var importedFolders = CreateDataTypeFolderStructure(dataTypeElements);

            foreach (var dataTypeElement in dataTypeElements)
            {
                var dataTypeDefinitionName = dataTypeElement.AttributeValue<string>("Name");

                var dataTypeDefinitionId = dataTypeElement.AttributeValue<Guid>("Definition");
                var databaseTypeAttribute = dataTypeElement.Attribute("DatabaseType");

                var parentId = -1;
                if (importedFolders.ContainsKey(dataTypeDefinitionName))
                    parentId = importedFolders[dataTypeDefinitionName];

                var definition = _dataTypeService.GetDataType(dataTypeDefinitionId);
                //If the datatype definition doesn't already exist we create a new according to the one in the package xml
                if (definition == null)
                {
                    var databaseType = databaseTypeAttribute?.Value.EnumParse<ValueStorageType>(true) ?? ValueStorageType.Ntext;

                    // the Id field is actually the string property editor Alias
                    // however, the actual editor with this alias could be installed with the package, and
                    // therefore not yet part of the _propertyEditors collection, so we cannot try and get
                    // the actual editor - going with a void editor

                    var editorAlias = dataTypeElement.Attribute("Id")?.Value?.Trim();
                    if (!_propertyEditors.TryGet(editorAlias, out var editor))
                        editor = new VoidEditor(_logger) { Alias = editorAlias };

                    var dataType = new DataType(editor)
                    {
                        Key = dataTypeDefinitionId,
                        Name = dataTypeDefinitionName,
                        DatabaseType = databaseType,
                        ParentId = parentId
                    };

                    var configurationAttributeValue = dataTypeElement.Attribute("Configuration")?.Value;
                    if (!string.IsNullOrWhiteSpace(configurationAttributeValue))
                        dataType.Configuration = editor.GetConfigurationEditor().FromDatabase(configurationAttributeValue);

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
                _dataTypeService.Save(dataTypes, userId, true);
            }

            return dataTypes;
        }

        private Dictionary<string, int> CreateDataTypeFolderStructure(IEnumerable<XElement> datatypeElements)
        {
            var importedFolders = new Dictionary<string, int>();
            foreach (var datatypeElement in datatypeElements)
            {
                var foldersAttribute = datatypeElement.Attribute("Folders");
                if (foldersAttribute != null)
                {
                    var name = datatypeElement.Attribute("Name").Value;
                    var folders = foldersAttribute.Value.Split(Constants.CharArrays.ForwardSlash);
                    var rootFolder = HttpUtility.UrlDecode(folders[0]);
                    //there will only be a single result by name for level 1 (root) containers
                    var current = _dataTypeService.GetContainers(rootFolder, 1).FirstOrDefault();

                    if (current == null)
                    {
                        var tryCreateFolder = _dataTypeService.CreateContainer(-1, rootFolder);
                        if (tryCreateFolder == false)
                        {
                            _logger.Error<PackagingService, string>(tryCreateFolder.Exception, "Could not create folder: {FolderName}", rootFolder);
                            throw tryCreateFolder.Exception;
                        }
                        current = _dataTypeService.GetContainer(tryCreateFolder.Result.Entity.Id);
                    }

                    importedFolders.Add(name, current.Id);

                    for (var i = 1; i < folders.Length; i++)
                    {
                        var folderName = HttpUtility.UrlDecode(folders[i]);
                        current = CreateDataTypeChildFolder(folderName, current);
                        importedFolders[name] = current.Id;
                    }
                }
            }

            return importedFolders;
        }

        private EntityContainer CreateDataTypeChildFolder(string folderName, IUmbracoEntity current)
        {
            var children = _entityService.GetChildren(current.Id).ToArray();
            var found = children.Any(x => x.Name.InvariantEquals(folderName));
            if (found)
            {
                var containerId = children.Single(x => x.Name.InvariantEquals(folderName)).Id;
                return _dataTypeService.GetContainer(containerId);
            }

            var tryCreateFolder = _dataTypeService.CreateContainer(current.Id, folderName);
            if (tryCreateFolder == false)
            {
                _logger.Error<PackagingService, string>(tryCreateFolder.Exception, "Could not create folder: {FolderName}", folderName);
                throw tryCreateFolder.Exception;
            }
            return _dataTypeService.GetContainer(tryCreateFolder.Result.Entity.Id);
        }

        #endregion

        #region Dictionary Items

        /// <summary>
        /// Imports and saves the 'DictionaryItems' part of the package xml as a list of <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="dictionaryItemElementList">Xml to import</param>
        /// <param name="userId"></param>
        /// <returns>An enumerable list of dictionary items</returns>
        public IReadOnlyList<IDictionaryItem> ImportDictionaryItems(IEnumerable<XElement> dictionaryItemElementList, int userId)
        {
            var languages = _localizationService.GetAllLanguages().ToList();
            return ImportDictionaryItems(dictionaryItemElementList, languages, null, userId);
        }

        private IReadOnlyList<IDictionaryItem> ImportDictionaryItems(IEnumerable<XElement> dictionaryItemElementList, List<ILanguage> languages, Guid? parentId, int userId)
        {
            var items = new List<IDictionaryItem>();
            foreach (var dictionaryItemElement in dictionaryItemElementList)
                items.AddRange(ImportDictionaryItem(dictionaryItemElement, languages, parentId, userId));

            return items;
        }

        private IEnumerable<IDictionaryItem> ImportDictionaryItem(XElement dictionaryItemElement, List<ILanguage> languages, Guid? parentId, int userId)
        {
            var items = new List<IDictionaryItem>();

            IDictionaryItem dictionaryItem;
            var key = dictionaryItemElement.Attribute("Key").Value;
            if (_localizationService.DictionaryItemExists(key))
                dictionaryItem = GetAndUpdateDictionaryItem(key, dictionaryItemElement, languages);
            else
                dictionaryItem = CreateNewDictionaryItem(key, dictionaryItemElement, languages, parentId);
            _localizationService.Save(dictionaryItem, userId);
            items.Add(dictionaryItem);

            items.AddRange(ImportDictionaryItems(dictionaryItemElement.Elements("DictionaryItem"), languages, dictionaryItem.Key, userId));
            return items;
        }

        private IDictionaryItem GetAndUpdateDictionaryItem(string key, XElement dictionaryItemElement, List<ILanguage> languages)
        {
            var dictionaryItem = _localizationService.GetDictionaryItemByKey(key);
            var translations = dictionaryItem.Translations.ToList();
            foreach (var valueElement in dictionaryItemElement.Elements("Value").Where(v => DictionaryValueIsNew(translations, v)))
                AddDictionaryTranslation(translations, valueElement, languages);
            dictionaryItem.Translations = translations;
            return dictionaryItem;
        }

        private static DictionaryItem CreateNewDictionaryItem(string key, XElement dictionaryItemElement, List<ILanguage> languages, Guid? parentId)
        {
            var dictionaryItem = parentId.HasValue ? new DictionaryItem(parentId.Value, key) : new DictionaryItem(key);
            var translations = new List<IDictionaryTranslation>();

            foreach (var valueElement in dictionaryItemElement.Elements("Value"))
                AddDictionaryTranslation(translations, valueElement, languages);

            dictionaryItem.Translations = translations;
            return dictionaryItem;
        }

        private static bool DictionaryValueIsNew(IEnumerable<IDictionaryTranslation> translations, XElement valueElement)
        {
            return translations.All(t =>
                String.Compare(t.Language.IsoCode, valueElement.Attribute("LanguageCultureAlias").Value,
                    StringComparison.InvariantCultureIgnoreCase) != 0
                );
        }

        private static void AddDictionaryTranslation(ICollection<IDictionaryTranslation> translations, XElement valueElement, IEnumerable<ILanguage> languages)
        {
            var languageId = valueElement.Attribute("LanguageCultureAlias").Value;
            var language = languages.SingleOrDefault(l => l.IsoCode == languageId);
            if (language == null)
                return;
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
            foreach (var languageElement in languageElements)
            {
                var isoCode = languageElement.AttributeValue<string>("CultureAlias");
                var existingLanguage = _localizationService.GetLanguageByIsoCode(isoCode);
                if (existingLanguage != null) continue;
                var langauge = new Language(isoCode)
                {
                    CultureName = languageElement.AttributeValue<string>("FriendlyName")
                };
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
        public IReadOnlyList<IMacro> ImportMacros(IEnumerable<XElement> macroElements, int userId)
        {
            var macros = macroElements.Select(ParseMacroElement).ToList();

            foreach (var macro in macros)
            {
                var existing = _macroService.GetByAlias(macro.Alias);
                if (existing != null)
                    macro.Id = existing.Id;

                _macroService.Save(macro, userId);
            }

            return macros;
        }

        private IMacro ParseMacroElement(XElement macroElement)
        {
            var macroName = macroElement.Element("name").Value;
            var macroAlias = macroElement.Element("alias").Value;
            var macroType = Enum<MacroTypes>.Parse(macroElement.Element("macroType").Value);
            var macroSource = macroElement.Element("macroSource").Value;

            //Following xml elements are treated as nullable properties
            var useInEditorElement = macroElement.Element("useInEditor");
            var useInEditor = false;
            if (useInEditorElement != null && string.IsNullOrEmpty((string)useInEditorElement) == false)
            {
                useInEditor = bool.Parse(useInEditorElement.Value);
            }
            var cacheDurationElement = macroElement.Element("refreshRate");
            var cacheDuration = 0;
            if (cacheDurationElement != null && string.IsNullOrEmpty((string)cacheDurationElement) == false)
            {
                cacheDuration = int.Parse(cacheDurationElement.Value);
            }
            var cacheByMemberElement = macroElement.Element("cacheByMember");
            var cacheByMember = false;
            if (cacheByMemberElement != null && string.IsNullOrEmpty((string)cacheByMemberElement) == false)
            {
                cacheByMember = bool.Parse(cacheByMemberElement.Value);
            }
            var cacheByPageElement = macroElement.Element("cacheByPage");
            var cacheByPage = false;
            if (cacheByPageElement != null && string.IsNullOrEmpty((string)cacheByPageElement) == false)
            {
                cacheByPage = bool.Parse(cacheByPageElement.Value);
            }
            var dontRenderElement = macroElement.Element("dontRender");
            var dontRender = true;
            if (dontRenderElement != null && string.IsNullOrEmpty((string)dontRenderElement) == false)
            {
                dontRender = bool.Parse(dontRenderElement.Value);
            }

            var existingMacro = _macroService.GetByAlias(macroAlias) as Macro;
            var macro = existingMacro ?? new Macro(macroAlias, macroName, macroSource, macroType,
                cacheByPage, cacheByMember, dontRender, useInEditor, cacheDuration);

            var properties = macroElement.Element("properties");
            if (properties != null)
            {
                int sortOrder = 0;
                foreach (var property in properties.Elements())
                {
                    var propertyName = property.Attribute("name").Value;
                    var propertyAlias = property.Attribute("alias").Value;
                    var editorAlias = property.Attribute("propertyType").Value;
                    var sortOrderAttribute = property.Attribute("sortOrder");
                    if (sortOrderAttribute != null)
                    {
                        sortOrder = int.Parse(sortOrderAttribute.Value);
                    }

                    if (macro.Properties.Values.Any(x => string.Equals(x.Alias, propertyAlias, StringComparison.OrdinalIgnoreCase))) continue;
                    macro.Properties.Add(new MacroProperty(propertyAlias, propertyName, sortOrder, editorAlias));
                    sortOrder++;
                }
            }
            return macro;
        }





        #endregion

        #region Stylesheets

        public IReadOnlyList<IFile> ImportStylesheets(IEnumerable<XElement> stylesheetElements, int userId)
        {
            var result = new List<IFile>();

            foreach (var n in stylesheetElements)
            {
                var stylesheetName = n.Element("Name")?.Value;
                if (stylesheetName.IsNullOrWhiteSpace()) continue;

                var s = _fileService.GetStylesheetByName(stylesheetName);
                if (s == null)
                {
                    var fileName = n.Element("FileName")?.Value;
                    if (fileName == null) continue;
                    var content = n.Element("Content")?.Value;
                    if (content == null) continue;

                    s = new Stylesheet(fileName) { Content = content };
                    _fileService.SaveStylesheet(s);
                }

                foreach (var prop in n.XPathSelectElements("Properties/Property"))
                {
                    var alias = prop.Element("Alias")?.Value;
                    var sp = s.Properties.SingleOrDefault(p => p != null && p.Alias == alias);
                    var name = prop.Element("Name")?.Value;
                    if (sp == null)
                    {
                        sp = new StylesheetProperty(name, "#" + name.ToSafeAlias(), "");
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
                    sp.Value = prop.Element("Value")?.Value;
                }
                _fileService.SaveStylesheet(s);
                result.Add(s);
            }

            return result;
        }

        #endregion

        #region Templates

        public IEnumerable<ITemplate> ImportTemplate(XElement templateElement, int userId)
        {
            return ImportTemplates(new[] { templateElement }, userId);
        }

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

            foreach (var tempElement in templateElements)
            {
                var dependencies = new List<string>();
                var elementCopy = tempElement;
                //Ensure that the Master of the current template is part of the import, otherwise we ignore this dependency as part of the dependency sorting.
                if (string.IsNullOrEmpty((string)elementCopy.Element("Master")) == false &&
                    templateElements.Any(x => (string)x.Element("Alias") == (string)elementCopy.Element("Master")))
                {
                    dependencies.Add((string)elementCopy.Element("Master"));
                }
                else if (string.IsNullOrEmpty((string)elementCopy.Element("Master")) == false &&
                         templateElements.Any(x => (string)x.Element("Alias") == (string)elementCopy.Element("Master")) == false)
                {
                    _logger.Info<PackageDataInstallation, string, string>(
                        "Template '{TemplateAlias}' has an invalid Master '{TemplateMaster}', so the reference has been ignored.",
                        (string)elementCopy.Element("Alias"),
                        (string)elementCopy.Element("Master"));
                }

                graph.AddItem(TopoGraph.CreateNode((string)elementCopy.Element("Alias"), elementCopy, dependencies));
            }

            //Sort templates by dependencies to a potential master template
            var sorted = graph.GetSortedItems();
            foreach (var item in sorted)
            {
                var templateElement = item.Item;

                var templateName = templateElement.Element("Name").Value;
                var alias = templateElement.Element("Alias").Value;
                var design = templateElement.Element("Design").Value;
                var masterElement = templateElement.Element("Master");

                var existingTemplate = _fileService.GetTemplate(alias) as Template;
                var template = existingTemplate ?? new Template(templateName, alias);
                template.Content = design;
                if (masterElement != null && string.IsNullOrEmpty((string)masterElement) == false)
                {
                    template.MasterTemplateAlias = masterElement.Value;
                    var masterTemplate = templates.FirstOrDefault(x => x.Alias == masterElement.Value);
                    if (masterTemplate != null)
                        template.MasterTemplateId = new Lazy<int>(() => masterTemplate.Id);
                }
                templates.Add(template);
            }

            if (templates.Any())
                _fileService.SaveTemplate(templates, userId);

            return templates;
        }

        private string ViewPath(string alias)
        {
            return SystemDirectories.MvcViews + "/" + alias.Replace(" ", "") + ".cshtml";
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core.Collections;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Strings;
using Content = Umbraco.Core.Models.Content;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents the Packaging Service, which provides import/export functionality for the Core models of the API
    /// using xml representation. This is primarily used by the Package functionality.
    /// </summary>
    public class PackagingService : IPackagingService
    {
        //fixme: inject when ready to use this
        private IPackageInstallation _packageInstallation;

        private readonly ILogger _logger;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IMacroService _macroService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IFileService _fileService;
        private readonly ILocalizationService _localizationService;
        private readonly IEntityService _entityService;
        private readonly IScopeProvider _scopeProvider;
        private Dictionary<string, IContentType> _importedContentTypes;
        private readonly IAuditRepository _auditRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ICreatedPackagesRepository _createdPackages;
        private static HttpClient _httpClient;

        public PackagingService(
            ILogger logger,
            IContentService contentService,
            IContentTypeService contentTypeService,
            IMacroService macroService,
            IDataTypeService dataTypeService,
            IFileService fileService,
            ILocalizationService localizationService,
            IEntityService entityService,
            IScopeProvider scopeProvider,
            IAuditRepository auditRepository,
            IContentTypeRepository contentTypeRepository,
            PropertyEditorCollection propertyEditors,
            ICreatedPackagesRepository createdPackages)
        {
            _logger = logger;
            _contentService = contentService;
            _contentTypeService = contentTypeService;
            _macroService = macroService;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _localizationService = localizationService;
            _entityService = entityService;
            _scopeProvider = scopeProvider;
            _auditRepository = auditRepository;
            _contentTypeRepository = contentTypeRepository;
            _propertyEditors = propertyEditors;
            _createdPackages = createdPackages;
            _importedContentTypes = new Dictionary<string, IContentType>();
        }

        protected IQuery<T> Query<T>() => _scopeProvider.SqlContext.Query<T>();

        #region Content

        



        /// <summary>
        /// Imports and saves package xml as <see cref="IContent"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="parentId">Optional parent Id for the content being imported</param>
        /// <param name="userId">Optional Id of the user performing the import</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated content</returns>
        public IEnumerable<IContent> ImportContent(XElement element, int parentId = -1, int userId = 0, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ImportingContent.IsRaisedEventCancelled(new ImportEventArgs<IContent>(element), this))
                    return Enumerable.Empty<IContent>();
            }

            var name = element.Name.LocalName;
            if (name.Equals("DocumentSet"))
            {
                //This is a regular deep-structured import
                var roots = from doc in element.Elements()
                            where (string)doc.Attribute("isDoc") == ""
                            select doc;

                var contents = ParseDocumentRootXml(roots, parentId).ToList();
                if (contents.Any())
                    _contentService.Save(contents, userId);

                if (raiseEvents)
                    ImportedContent.RaiseEvent(new ImportEventArgs<IContent>(contents, element, false), this);
                return contents;
            }

            var attribute = element.Attribute("isDoc");
            if (attribute != null)
            {
                //This is a single doc import
                var elements = new List<XElement> { element };
                var contents = ParseDocumentRootXml(elements, parentId).ToList();
                if (contents.Any())
                    _contentService.Save(contents, userId);

                if (raiseEvents)
                    ImportedContent.RaiseEvent(new ImportEventArgs<IContent>(contents, element, false), this);
                return contents;
            }

            throw new ArgumentException(
                "The passed in XElement is not valid! It does not contain a root element called " +
                "'DocumentSet' (for structured imports) nor is the first element a Document (for single document import).");
        }

        private IEnumerable<IContent> ParseDocumentRootXml(IEnumerable<XElement> roots, int parentId)
        {
            var contents = new List<IContent>();
            foreach (var root in roots)
            {
                var contentTypeAlias = root.Name.LocalName;

                if (_importedContentTypes.ContainsKey(contentTypeAlias) == false)
                {
                    var contentType = FindContentTypeByAlias(contentTypeAlias);
                    _importedContentTypes.Add(contentTypeAlias, contentType);
                }

                var content = CreateContentFromXml(root, _importedContentTypes[contentTypeAlias], null, parentId);
                contents.Add(content);

                var children = (from child in root.Elements()
                               where (string)child.Attribute("isDoc") == ""
                               select child)
                    .ToList();
                if (children.Any())
                    contents.AddRange(CreateContentFromXml(children, content));
            }
            return contents;
        }

        private IEnumerable<IContent> CreateContentFromXml(IEnumerable<XElement> children, IContent parent)
        {
            var list = new List<IContent>();
            foreach (var child in children)
            {
                string contentTypeAlias = child.Name.LocalName;

                if (_importedContentTypes.ContainsKey(contentTypeAlias) == false)
                {
                    var contentType = FindContentTypeByAlias(contentTypeAlias);
                    _importedContentTypes.Add(contentTypeAlias, contentType);
                }

                //Create and add the child to the list
                var content = CreateContentFromXml(child, _importedContentTypes[contentTypeAlias], parent, default(int));
                list.Add(content);

                //Recursive call
                XElement child1 = child;
                var grandChildren = (from grand in child1.Elements()
                    where (string) grand.Attribute("isDoc") == ""
                    select grand).ToList();

                if (grandChildren.Any())
                    list.AddRange(CreateContentFromXml(grandChildren, content));
            }

            return list;
        }

        private IContent CreateContentFromXml(XElement element, IContentType contentType, IContent parent, int parentId)
        {
            var id = element.Attribute("id").Value;
            var level = element.Attribute("level").Value;
            var sortOrder = element.Attribute("sortOrder").Value;
            var nodeName = element.Attribute("nodeName").Value;
            var path = element.Attribute("path").Value;
            //TODO: Shouldn't we be using this value???
            var template = element.Attribute("template").Value;
            var key = Guid.Empty;

            var properties = from property in element.Elements()
                             where property.Attribute("isDoc") == null
                             select property;

            IContent content = parent == null
                                   ? new Content(nodeName, parentId, contentType)
                                   {
                                       Level = int.Parse(level),
                                       SortOrder = int.Parse(sortOrder)
                                   }
                                   : new Content(nodeName, parent, contentType)
                                   {
                                       Level = int.Parse(level),
                                       SortOrder = int.Parse(sortOrder)
                                   };

            if (element.Attribute("key") != null && Guid.TryParse(element.Attribute("key").Value, out key))
            {
                // update the Guid (for UDI support)
                content.Key = key;
            }

            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                foreach (var property in properties)
                {
                    string propertyTypeAlias = property.Name.LocalName;
                    if (content.HasProperty(propertyTypeAlias))
                    {
                        var propertyValue = property.Value;

                        var propertyType = contentType.PropertyTypes.FirstOrDefault(pt => pt.Alias == propertyTypeAlias);

                        //TODO: It would be heaps nicer if we didn't have to hard code references to specific property editors
                        // we'd have to modify the packaging format to denote how to parse/store the value instead of relying on this

                        if (propertyType != null)
                        {
                            // fixme - wtf is this very specific thing here?!
                            //if (propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.CheckBoxList)
                            //{

                            //    //TODO: We need to refactor this so the packager isn't making direct db calls for an 'edge' case
                            //    var database = scope.Database;
                            //    var dtos = database.Fetch<DataTypePreValueDto>("WHERE datatypeNodeId = @Id", new { Id = propertyType.DataTypeId });

                            //    var propertyValueList = new List<string>();
                            //    foreach (var preValue in propertyValue.Split(','))
                            //    {
                            //        propertyValueList.Add(dtos.Single(x => x.Value == preValue).Id.ToString(CultureInfo.InvariantCulture));
                            //    }

                            //    propertyValue = string.Join(",", propertyValueList.ToArray());

                            //}
                        }
                        //set property value
                        content.SetValue(propertyTypeAlias, propertyValue);
                    }
                }
            }

            return content;
        }

        #endregion

        #region ContentTypes

        

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated ContentTypes</returns>
        public IEnumerable<IContentType> ImportContentTypes(XElement element, int userId = 0, bool raiseEvents = true)
        {
            return ImportContentTypes(element, true, userId);
        }

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="importStructure">Boolean indicating whether or not to import the </param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated ContentTypes</returns>
        public IEnumerable<IContentType> ImportContentTypes(XElement element, bool importStructure, int userId = 0, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ImportingContentType.IsRaisedEventCancelled(new ImportEventArgs<IContentType>(element), this))
                    return Enumerable.Empty<IContentType>();
            }

            var name = element.Name.LocalName;
            if (name.Equals("DocumentTypes") == false && name.Equals("DocumentType") == false)
            {
                throw new ArgumentException("The passed in XElement is not valid! It does not contain a root element called 'DocumentTypes' for multiple imports or 'DocumentType' for a single import.");
            }

            _importedContentTypes = new Dictionary<string, IContentType>();
            var unsortedDocumentTypes = name.Equals("DocumentTypes")
                                    ? (from doc in element.Elements("DocumentType") select doc).ToList()
                                    : new List<XElement> { element };

            //When you are importing a single doc type we have to assume that the depedencies are already there.
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
                if (_importedContentTypes.ContainsKey(alias) == false)
                {
                    var contentType = _contentTypeService.Get(alias);
                    _importedContentTypes.Add(alias, contentType == null
                                                         ? CreateContentTypeFromXml(documentType)
                                                         : UpdateContentTypeFromXml(documentType, contentType));
                }
            }

            foreach (var contentType in _importedContentTypes)
            {
                var ct = contentType.Value;
                if (importedFolders.ContainsKey(ct.Alias))
                {
                    ct.ParentId = importedFolders[ct.Alias];
                }
            }

            //Save the newly created/updated IContentType objects
            var list = _importedContentTypes.Select(x => x.Value).ToList();
            _contentTypeService.Save(list, userId);

            //Now we can finish the import by updating the 'structure',
            //which requires the doc types to be saved/available in the db
            if (importStructure)
            {
                var updatedContentTypes = new List<IContentType>();
                //Update the structure here - we can't do it untill all DocTypes have been created
                foreach (var documentType in documentTypes)
                {
                    var alias = documentType.Element("Info").Element("Alias").Value;
                    var structureElement = documentType.Element("Structure");
                    //Ensure that we only update ContentTypes which has actual structure-elements
                    if (structureElement == null || structureElement.Elements("DocumentType").Any() == false) continue;

                    var updated = UpdateContentTypesStructure(_importedContentTypes[alias], structureElement);
                    updatedContentTypes.Add(updated);
                }
                //Update ContentTypes with a newly added structure/list of allowed children
                if (updatedContentTypes.Any())
                    _contentTypeService.Save(updatedContentTypes, userId);
            }

            if (raiseEvents)
                ImportedContentType.RaiseEvent(new ImportEventArgs<IContentType>(list, element, false), this);

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
                    var folders = foldersAttribute.Value.Split('/');
                    var rootFolder = HttpUtility.UrlDecode(folders[0]);
                    //level 1 = root level folders, there can only be one with the same name
                    var current = _contentTypeService.GetContainers(rootFolder, 1).FirstOrDefault();

                    if (current == null)
                    {
                        var tryCreateFolder = _contentTypeService.CreateContainer(-1, rootFolder);
                        if (tryCreateFolder == false)
                        {
                            _logger.Error<PackagingService>(tryCreateFolder.Exception, "Could not create folder: {FolderName}", rootFolder);
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
                _logger.Error<PackagingService>(tryCreateFolder.Exception, "Could not create folder: {FolderName}", folderName);
                throw tryCreateFolder.Exception;
            }
            return _contentTypeService.GetContainer(tryCreateFolder.Result.Entity.Id);
        }

        private IContentType CreateContentTypeFromXml(XElement documentType)
        {
            var infoElement = documentType.Element("Info");

            //Name of the master corresponds to the parent
            var masterElement = infoElement.Element("Master");
            IContentType parent = null;
            if (masterElement != null)
            {
                var masterAlias = masterElement.Value;
                parent = _importedContentTypes.ContainsKey(masterAlias)
                             ? _importedContentTypes[masterAlias]
                             : _contentTypeService.Get(masterAlias);
            }

            var alias = infoElement.Element("Alias").Value;
            var contentType = parent == null
                                  ? new ContentType(-1) { Alias = alias }
                                  : new ContentType(parent, alias);

            if (parent != null)
                contentType.AddContentType(parent);

            return UpdateContentTypeFromXml(documentType, contentType);
        }

        private IContentType UpdateContentTypeFromXml(XElement documentType, IContentType contentType)
        {
            var infoElement = documentType.Element("Info");
            var defaultTemplateElement = infoElement.Element("DefaultTemplate");

            contentType.Name = infoElement.Element("Name").Value;
            contentType.Icon = infoElement.Element("Icon").Value;
            contentType.Thumbnail = infoElement.Element("Thumbnail").Value;
            contentType.Description = infoElement.Element("Description").Value;

            //NOTE AllowAtRoot is a new property in the package xml so we need to verify it exists before using it.
            var allowAtRoot = infoElement.Element("AllowAtRoot");
            if (allowAtRoot != null)
                contentType.AllowedAsRoot = allowAtRoot.Value.InvariantEquals("true");

            //NOTE IsListView is a new property in the package xml so we need to verify it exists before using it.
            var isListView = infoElement.Element("IsListView");
            if (isListView != null)
                contentType.IsContainer = isListView.Value.InvariantEquals("true");

            //Name of the master corresponds to the parent and we need to ensure that the Parent Id is set
            var masterElement = infoElement.Element("Master");
            if (masterElement != null)
            {
                var masterAlias = masterElement.Value;
                IContentType parent = _importedContentTypes.ContainsKey(masterAlias)
                    ? _importedContentTypes[masterAlias]
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
                        var compositionContentType = _importedContentTypes.ContainsKey(compositionAlias)
                            ? _importedContentTypes[compositionAlias]
                            : _contentTypeService.Get(compositionAlias);
                        var added = contentType.AddContentType(compositionContentType);
                    }
                }
            }

            UpdateContentTypesAllowedTemplates(contentType, infoElement.Element("AllowedTemplates"), defaultTemplateElement);
            UpdateContentTypesTabs(contentType, documentType.Element("Tabs"));
            UpdateContentTypesProperties(contentType, documentType.Element("GenericProperties"));

            return contentType;
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
                        _logger.Warn<PackagingService>("Packager: Error handling allowed templates. Template with alias '{TemplateAlias}' could not be found.", alias);
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
                    _logger.Warn<PackagingService>("Packager: Error handling default template. Default template with alias '{DefaultTemplateAlias}' could not be found.", defaultTemplateElement.Value);
                }
            }
        }

        private void UpdateContentTypesTabs(IContentType contentType, XElement tabElement)
        {
            if (tabElement == null)
                return;

            var tabs = tabElement.Elements("Tab");
            foreach (var tab in tabs)
            {
                var id = tab.Element("Id").Value;//Do we need to use this for tracking?
                var caption = tab.Element("Caption").Value;

                if (contentType.PropertyGroups.Contains(caption) == false)
                {
                    contentType.AddPropertyGroup(caption);

                }

                int sortOrder;
                if (tab.Element("SortOrder") != null && int.TryParse(tab.Element("SortOrder").Value, out sortOrder))
                {
                    // Override the sort order with the imported value
                    contentType.PropertyGroups[caption].SortOrder = sortOrder;
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
                    _logger.Warn<PackagingService>("Packager: Error handling creation of PropertyType '{PropertyType}'. Could not find DataTypeDefintion with unique id '{DataTypeDefinitionId}' nor one referencing the DataType with a property editor alias (or legacy control id) '{PropertyEditorAlias}'. Did the package creator forget to package up custom datatypes? This property will be converted to a label/readonly editor if one exists.",
                        property.Element("Name").Value, dataTypeDefinitionId, property.Element("Type").Value.Trim());

                    //convert to a label!
                    dataTypeDefinition = _dataTypeService.GetByEditorAlias(Constants.PropertyEditors.Aliases.NoEdit).FirstOrDefault();
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
                    Mandatory = property.Element("Mandatory") != null ? property.Element("Mandatory").Value.ToLowerInvariant().Equals("true") : false,
                    ValidationRegExp = (string)property.Element("Validation"),
                    SortOrder = sortOrder
                };

                var tab = (string)property.Element("Tab");
                if (string.IsNullOrEmpty(tab))
                {
                    contentType.AddPropertyType(propertyType);
                }
                else
                {
                    contentType.AddPropertyType(propertyType, tab);
                }
            }
        }

        private IContentType UpdateContentTypesStructure(IContentType contentType, XElement structureElement)
        {
            var allowedChildren = contentType.AllowedContentTypes.ToList();
            int sortOrder = allowedChildren.Any() ? allowedChildren.Last().SortOrder : 0;
            foreach (var element in structureElement.Elements("DocumentType"))
            {
                var alias = element.Value;

                var allowedChild = _importedContentTypes.ContainsKey(alias) ? _importedContentTypes[alias] : _contentTypeService.Get(alias);
                if (allowedChild == null)
                {
                    _logger.Warn<PackagingService>(
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
            using (var scope = _scopeProvider.CreateScope())
            {
                var query = Query<IContentType>().Where(x => x.Alias == contentTypeAlias);
                var contentType = _contentTypeRepository.Get(query).FirstOrDefault();

                if (contentType == null)
                    throw new Exception($"ContentType matching the passed in Alias: '{contentTypeAlias}' was null");

                scope.Complete();
                return contentType;
            }
        }

        #endregion

        #region DataTypes

        

        

        /// <summary>
        /// Imports and saves package xml as <see cref="IDataType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the user</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated DataTypeDefinitions</returns>
        public IEnumerable<IDataType> ImportDataTypeDefinitions(XElement element, int userId = 0, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ImportingDataType.IsRaisedEventCancelled(new ImportEventArgs<IDataType>(element), this))
                    return Enumerable.Empty<IDataType>();
            }

            var name = element.Name.LocalName;
            if (name.Equals("DataTypes") == false && name.Equals("DataType") == false)
            {
                throw new ArgumentException("The passed in XElement is not valid! It does not contain a root element called 'DataTypes' for multiple imports or 'DataType' for a single import.");
            }

            var dataTypes = new List<IDataType>();
            var dataTypeElements = name.Equals("DataTypes")
                                       ? (from doc in element.Elements("DataType") select doc).ToList()
                                       : new List<XElement> { element };

            var importedFolders = CreateDataTypeFolderStructure(dataTypeElements);

            foreach (var dataTypeElement in dataTypeElements)
            {
                var dataTypeDefinitionName = dataTypeElement.Attribute("Name").Value;

                var dataTypeDefinitionId = new Guid(dataTypeElement.Attribute("Definition").Value);
                var databaseTypeAttribute = dataTypeElement.Attribute("DatabaseType");

                var parentId = -1;
                if (importedFolders.ContainsKey(dataTypeDefinitionName))
                    parentId = importedFolders[dataTypeDefinitionName];

                var definition = _dataTypeService.GetDataType(dataTypeDefinitionId);
                //If the datatypedefinition doesn't already exist we create a new new according to the one in the package xml
                if (definition == null)
                {
                    var databaseType = databaseTypeAttribute != null
                                           ? databaseTypeAttribute.Value.EnumParse<ValueStorageType>(true)
                                           : ValueStorageType.Ntext;

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

            if (raiseEvents)
                ImportedDataType.RaiseEvent(new ImportEventArgs<IDataType>(dataTypes, element, false), this);

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
                    var folders = foldersAttribute.Value.Split('/');
                    var rootFolder = HttpUtility.UrlDecode(folders[0]);
                    //there will only be a single result by name for level 1 (root) containers
                    var current = _dataTypeService.GetContainers(rootFolder, 1).FirstOrDefault();

                    if (current == null)
                    {
                        var tryCreateFolder = _dataTypeService.CreateContainer(-1, rootFolder);
                        if (tryCreateFolder == false)
                        {
                            _logger.Error<PackagingService>(tryCreateFolder.Exception, "Could not create folder: {FolderName}", rootFolder);
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
                _logger.Error<PackagingService>(tryCreateFolder.Exception, "Could not create folder: {FolderName}", folderName);
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
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumerable list of dictionary items</returns>
        public IEnumerable<IDictionaryItem> ImportDictionaryItems(XElement dictionaryItemElementList, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ImportingDictionaryItem.IsRaisedEventCancelled(new ImportEventArgs<IDictionaryItem>(dictionaryItemElementList), this))
                    return Enumerable.Empty<IDictionaryItem>();
            }

            var languages = _localizationService.GetAllLanguages().ToList();
            return ImportDictionaryItems(dictionaryItemElementList, languages, raiseEvents);
        }

        private IEnumerable<IDictionaryItem> ImportDictionaryItems(XElement dictionaryItemElementList, List<ILanguage> languages, bool raiseEvents, Guid? parentId = null)
        {
            var items = new List<IDictionaryItem>();
            foreach (var dictionaryItemElement in dictionaryItemElementList.Elements("DictionaryItem"))
                items.AddRange(ImportDictionaryItem(dictionaryItemElement, languages, raiseEvents, parentId));


            if (raiseEvents)
                ImportedDictionaryItem.RaiseEvent(new ImportEventArgs<IDictionaryItem>(items, dictionaryItemElementList, false), this);

            return items;
        }

        private IEnumerable<IDictionaryItem> ImportDictionaryItem(XElement dictionaryItemElement, List<ILanguage> languages, bool raiseEvents, Guid? parentId)
        {
            var items = new List<IDictionaryItem>();

            IDictionaryItem dictionaryItem;
            var key = dictionaryItemElement.Attribute("Key").Value;
            if (_localizationService.DictionaryItemExists(key))
                dictionaryItem = GetAndUpdateDictionaryItem(key, dictionaryItemElement, languages);
            else
                dictionaryItem = CreateNewDictionaryItem(key, dictionaryItemElement, languages, parentId);
            _localizationService.Save(dictionaryItem);
            items.Add(dictionaryItem);

            items.AddRange(ImportDictionaryItems(dictionaryItemElement, languages, raiseEvents, dictionaryItem.Key));
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

        #region Files
        #endregion

        #region Languages

        

        

        /// <summary>
        /// Imports and saves the 'Languages' part of a package xml as a list of <see cref="ILanguage"/>
        /// </summary>
        /// <param name="languageElementList">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumerable list of generated languages</returns>
        public IEnumerable<ILanguage> ImportLanguages(XElement languageElementList, int userId = 0, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ImportingLanguage.IsRaisedEventCancelled(new ImportEventArgs<ILanguage>(languageElementList), this))
                    return Enumerable.Empty<ILanguage>();
            }

            var list = new List<ILanguage>();
            foreach (var languageElement in languageElementList.Elements("Language"))
            {
                var isoCode = languageElement.Attribute("CultureAlias").Value;
                var existingLanguage = _localizationService.GetLanguageByIsoCode(isoCode);
                if (existingLanguage == null)
                {
                    var langauge = new Language(isoCode)
                    {
                        CultureName = languageElement.Attribute("FriendlyName").Value
                    };
                    _localizationService.Save(langauge);
                    list.Add(langauge);
                }
            }

            if (raiseEvents)
                ImportedLanguage.RaiseEvent(new ImportEventArgs<ILanguage>(list, languageElementList, false), this);

            return list;
        }

        #endregion

        #region Macros

        /// <summary>
        /// Imports and saves the 'Macros' part of a package xml as a list of <see cref="IMacro"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns></returns>
        public IEnumerable<IMacro> ImportMacros(XElement element, int userId = 0, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ImportingMacro.IsRaisedEventCancelled(new ImportEventArgs<IMacro>(element), this))
                    return Enumerable.Empty<IMacro>();
            }

            var name = element.Name.LocalName;
            if (name.Equals("Macros") == false && name.Equals("macro") == false)
            {
                throw new ArgumentException("The passed in XElement is not valid! It does not contain a root element called 'Macros' for multiple imports or 'macro' for a single import.");
            }

            var macroElements = name.Equals("Macros")
                                       ? (from doc in element.Elements("macro") select doc).ToList()
                                       : new List<XElement> { element };

            var macros = macroElements.Select(ParseMacroElement).ToList();

            foreach (var macro in macros)
            {
                var existing = _macroService.GetByAlias(macro.Alias);
                if (existing != null)
                    macro.Id = existing.Id;

                _macroService.Save(macro, userId);
            }

            if (raiseEvents)
                ImportedMacro.RaiseEvent(new ImportEventArgs<IMacro>(macros, element, false), this);

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

        #region Package Files

        /// <summary>
        /// This will fetch an Umbraco package file from the package repository and return the relative file path to the downloaded package file
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="umbracoVersion"></param>
        /// /// <param name="userId">The current user id performing the operation</param>
        /// <returns></returns>
        public string FetchPackageFile(Guid packageId, Version umbracoVersion, int userId)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                //includeHidden = true because we don't care if it's hidden we want to get the file regardless
                var url = $"{Constants.PackageRepository.RestApiBaseUrl}/{packageId}?version={umbracoVersion.ToString(3)}&includeHidden=true&asFile=true";
                byte[] bytes;
                try
                {
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient();
                    }
                    bytes = _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                }
                catch (HttpRequestException ex)
                {
                    throw new ConnectionException("An error occuring downloading the package from " + url, ex);
                }

                //successfull
                if (bytes.Length > 0)
                {
                    var packagePath = IOHelper.MapPath(SystemDirectories.Packages);

                    // Check for package directory
                    if (Directory.Exists(packagePath) == false)
                        Directory.CreateDirectory(packagePath);

                    var packageFilePath = Path.Combine(packagePath, packageId + ".umb");

                    using (var fs1 = new FileStream(packageFilePath, FileMode.Create))
                    {
                        fs1.Write(bytes, 0, bytes.Length);
                        return "packages\\" + packageId + ".umb";
                    }
                }

                Audit(AuditType.PackagerInstall, $"Package {packageId} fetched from {Constants.PackageRepository.DefaultRepositoryId}", userId, -1);
                return null;
            }
        }

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, "Package", message));
        }

        #endregion

        #region Templates

        /// <summary>
        /// Imports and saves package xml as <see cref="ITemplate"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional user id</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated Templates</returns>
        public IEnumerable<ITemplate> ImportTemplates(XElement element, int userId = 0, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ImportingTemplate.IsRaisedEventCancelled(new ImportEventArgs<ITemplate>(element), this))
                    return Enumerable.Empty<ITemplate>();
            }

            var name = element.Name.LocalName;
            if (name.Equals("Templates") == false && name.Equals("Template") == false)
            {
                throw new ArgumentException("The passed in XElement is not valid! It does not contain a root element called 'Templates' for multiple imports or 'Template' for a single import.");
            }

            var templates = new List<ITemplate>();
            var templateElements = name.Equals("Templates")
                                       ? (from doc in element.Elements("Template") select doc).ToList()
                                       : new List<XElement> { element };

            var graph = new TopoGraph<string, TopoGraph.Node<string, XElement>>(x => x.Key, x => x.Dependencies);

            foreach (XElement tempElement in templateElements)
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
                    _logger.Info<PackagingService>(
                        "Template '{TemplateAlias}' has an invalid Master '{TemplateMaster}', so the reference has been ignored.",
                        (string) elementCopy.Element("Alias"),
                        (string) elementCopy.Element("Master"));
                }

                graph.AddItem(TopoGraph.CreateNode((string) elementCopy.Element("Alias"), elementCopy, dependencies));
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

                var isMasterPage = IsMasterPageSyntax(design);
                var path = isMasterPage ? MasterpagePath(alias) : ViewPath(alias);

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

            if (raiseEvents)
                ImportedTemplate.RaiseEvent(new ImportEventArgs<ITemplate>(templates, element, false), this);

            return templates;
        }


        private bool IsMasterPageSyntax(string code)
        {
            return Regex.IsMatch(code, @"<%@\s*Master", RegexOptions.IgnoreCase) ||
                code.InvariantContains("<umbraco:Item") || code.InvariantContains("<asp:") || code.InvariantContains("<umbraco:Macro");
        }

        private string ViewPath(string alias)
        {
            return SystemDirectories.MvcViews + "/" + alias.Replace(" ", "") + ".cshtml";
        }

        private string MasterpagePath(string alias)
        {
            return IOHelper.MapPath(SystemDirectories.Masterpages + "/" + alias.Replace(" ", "") + ".master");
        }

        #endregion

        #region Stylesheets

        public IEnumerable<IFile> ImportStylesheets(XElement element, int userId = 0, bool raiseEvents = true)
        {

            if (raiseEvents)
            {
                if (ImportingStylesheets.IsRaisedEventCancelled(new ImportEventArgs<IFile>(element), this))
                    return Enumerable.Empty<IFile>();
            }

            IEnumerable<IFile> styleSheets = Enumerable.Empty<IFile>();

            if (element.Elements().Any())
                throw new NotImplementedException("This needs to be implimentet");


            if (raiseEvents)
                ImportingStylesheets.RaiseEvent(new ImportEventArgs<IFile>(styleSheets, element, false), this);

            return styleSheets;

        }

        #endregion

        #region Installation

        //fixme: None of these methods are actually used! They have unit tests for them though, but we don't actively use this yet but we should!

        internal IPackageInstallation PackageInstallation
        {
            private get { return _packageInstallation ?? new PackageInstallation(this, _macroService, _fileService, new PackageExtraction()); }
            set { _packageInstallation = value; }
        }

        internal InstallationSummary InstallPackage(string packageFilePath, int userId = 0, bool raiseEvents = false)
        {
            var metaData = GetPackageMetaData(packageFilePath);

            if (raiseEvents)
            {
                if (ImportingPackage.IsRaisedEventCancelled(new ImportPackageEventArgs<string>(packageFilePath, metaData), this))
                {
                    var initEmpty = new InstallationSummary().InitEmpty();
                    initEmpty.MetaData = metaData;
                    return initEmpty;
                }
            }
            var installationSummary = PackageInstallation.InstallPackage(packageFilePath, userId);

            if (raiseEvents)
            {
                ImportedPackage.RaiseEvent(new ImportPackageEventArgs<InstallationSummary>(installationSummary, metaData, false), this);
            }

            return installationSummary;
        }

        internal PreInstallWarnings GetPackageWarnings(string packageFilePath)
        {
            return PackageInstallation.GetPreInstallWarnings(packageFilePath);
        }

        internal MetaData GetPackageMetaData(string packageFilePath)
        {
            return PackageInstallation.GetMetaData(packageFilePath);
        }

        #endregion

        #region Package Building

        public void DeleteCreatedPackage(int id) => _createdPackages.Delete(id);

        public IEnumerable<PackageDefinition> GetAllCreatedPackages() => _createdPackages.GetAll();

        public PackageDefinition GetCreatedPackageById(int id) => _createdPackages.GetById(id);

        public bool SaveCreatedPackage(PackageDefinition definition) => _createdPackages.SavePackage(definition);

        public string ExportCreatedPackage(PackageDefinition definition) => _createdPackages.ExportPackage(definition);

        #endregion

        /// <summary>
        /// This method can be used to trigger the 'ImportedPackage' event when a package is installed by something else but this service.
        /// </summary>
        /// <param name="args"></param>
        internal static void OnImportedPackage(ImportPackageEventArgs<InstallationSummary> args)
        {
            ImportedPackage.RaiseEvent(args, null);
        }

        /// <summary>
        /// This method can be used to trigger the 'UninstalledPackage' event when a package is uninstalled by something else but this service.
        /// </summary>
        /// <param name="args"></param>
        internal static void OnUninstalledPackage(UninstallPackageEventArgs<UninstallationSummary> args)
        {
            UninstalledPackage.RaiseEvent(args, null);
        }

        #region Event Handlers
        /// <summary>
        /// Occurs before Importing Content
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IContent>> ImportingContent;

        /// <summary>
        /// Occurs after Content is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IContent>> ImportedContent;

        /// <summary>
        /// Occurs before Importing ContentType
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IContentType>> ImportingContentType;

        /// <summary>
        /// Occurs after ContentType is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IContentType>> ImportedContentType;

        /// <summary>
        /// Occurs before Importing DataType
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IDataType>> ImportingDataType;

        /// <summary>
        /// Occurs after DataType is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IDataType>> ImportedDataType;

        /// <summary>
        /// Occurs before Importing DictionaryItem
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IDictionaryItem>> ImportingDictionaryItem;

        /// <summary>
        /// Occurs after DictionaryItem is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IDictionaryItem>> ImportedDictionaryItem;

        /// <summary>
        /// Occurs before Importing Macro
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IMacro>> ImportingMacro;

        /// <summary>
        /// Occurs after Macro is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IMacro>> ImportedMacro;

        /// <summary>
        /// Occurs before Importing Language
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<ILanguage>> ImportingLanguage;

        /// <summary>
        /// Occurs after Language is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<ILanguage>> ImportedLanguage;

        /// <summary>
        /// Occurs before Importing Template
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<ITemplate>> ImportingTemplate;

        /// <summary>
        /// Occurs before Importing Stylesheets
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IFile>> ImportingStylesheets;

        /// <summary>
        /// Occurs after Template is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<ITemplate>> ImportedTemplate;

        /// <summary>
        /// Occurs before Importing umbraco package
        /// </summary>
        internal static event TypedEventHandler<IPackagingService, ImportPackageEventArgs<string>> ImportingPackage;

        /// <summary>
        /// Occurs after a package is imported
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportPackageEventArgs<InstallationSummary>> ImportedPackage;

        /// <summary>
        /// Occurs after a package is uninstalled
        /// </summary>
        public static event TypedEventHandler<IPackagingService, UninstallPackageEventArgs<UninstallationSummary>> UninstalledPackage;

        #endregion

        
    }
}

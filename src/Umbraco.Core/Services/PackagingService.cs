using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Packaging;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Packaging Service, which provides import/export functionality for the Core models of the API
    /// using xml representation. This is primarily used by the Package functionality.
    /// </summary>
    public class PackagingService : IPackagingService
    {
        private readonly ILogger _logger;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaService _mediaService;
        private readonly IMacroService _macroService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IFileService _fileService;
        private readonly ILocalizationService _localizationService;
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private Dictionary<string, IContentType> _importedContentTypes;
        private IPackageInstallation _packageInstallation;
        private readonly IUserService _userService;


        public PackagingService(
            ILogger logger,
            IContentService contentService,
            IContentTypeService contentTypeService,
            IMediaService mediaService,
            IMacroService macroService,
            IDataTypeService dataTypeService,
            IFileService fileService,
            ILocalizationService localizationService,
            IUserService userService,
            RepositoryFactory repositoryFactory,
            IDatabaseUnitOfWorkProvider uowProvider)
        {
            _logger = logger;
            _contentService = contentService;
            _contentTypeService = contentTypeService;
            _mediaService = mediaService;
            _macroService = macroService;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _localizationService = localizationService;
            _repositoryFactory = repositoryFactory;
            _uowProvider = uowProvider;
            _userService = userService;
            _importedContentTypes = new Dictionary<string, IContentType>();
        }

        #region Content

        /// <summary>
        /// Exports an <see cref="IContent"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="content">Content to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Content object</returns>
        public XElement Export(IContent content, bool deep = false, bool raiseEvents = true)
        {
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : content.ContentType.Alias.ToSafeAliasWithForcingCheck();

            if (raiseEvents)
            {
                if (ExportingContent.IsRaisedEventCancelled(new ExportEventArgs<IContent>(content, nodeName), this))
                    return new XElement(nodeName);
            }

            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(_contentService, _dataTypeService, _userService, content, deep);

            if (raiseEvents)
                ExportedContent.RaiseEvent(new ExportEventArgs<IContent>(content, xml, false), this);

            return xml;
        }



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

                var contents = ParseDocumentRootXml(roots, parentId);
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
                var contents = ParseDocumentRootXml(elements, parentId);
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
                bool isLegacySchema = root.Name.LocalName.ToLowerInvariant().Equals("node");
                string contentTypeAlias = isLegacySchema
                                              ? root.Attribute("nodeTypeAlias").Value
                                              : root.Name.LocalName;

                if (_importedContentTypes.ContainsKey(contentTypeAlias) == false)
                {
                    var contentType = FindContentTypeByAlias(contentTypeAlias);
                    _importedContentTypes.Add(contentTypeAlias, contentType);
                }

                var content = CreateContentFromXml(root, _importedContentTypes[contentTypeAlias], null, parentId, isLegacySchema);
                contents.Add(content);

                var children = from child in root.Elements()
                               where (string)child.Attribute("isDoc") == ""
                               select child;
                if (children.Any())
                    contents.AddRange(CreateContentFromXml(children, content, isLegacySchema));
            }
            return contents;
        }

        private IEnumerable<IContent> CreateContentFromXml(IEnumerable<XElement> children, IContent parent, bool isLegacySchema)
        {
            var list = new List<IContent>();
            foreach (var child in children)
            {
                string contentTypeAlias = isLegacySchema
                                              ? child.Attribute("nodeTypeAlias").Value
                                              : child.Name.LocalName;

                if (_importedContentTypes.ContainsKey(contentTypeAlias) == false)
                {
                    var contentType = FindContentTypeByAlias(contentTypeAlias);
                    _importedContentTypes.Add(contentTypeAlias, contentType);
                }

                //Create and add the child to the list
                var content = CreateContentFromXml(child, _importedContentTypes[contentTypeAlias], parent, default(int), isLegacySchema);
                list.Add(content);

                //Recursive call
                XElement child1 = child;
                var grandChildren = from grand in child1.Elements()
                                    where (string)grand.Attribute("isDoc") == ""
                                    select grand;

                if (grandChildren.Any())
                    list.AddRange(CreateContentFromXml(grandChildren, content, isLegacySchema));
            }

            return list;
        }

        private IContent CreateContentFromXml(XElement element, IContentType contentType, IContent parent, int parentId, bool isLegacySchema)
        {
            var id = element.Attribute("id").Value;
            var level = element.Attribute("level").Value;
            var sortOrder = element.Attribute("sortOrder").Value;
            var nodeName = element.Attribute("nodeName").Value;
            var path = element.Attribute("path").Value;
            var template = element.Attribute("template").Value;

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

            foreach (var property in properties)
            {
                string propertyTypeAlias = isLegacySchema ? property.Attribute("alias").Value : property.Name.LocalName;
                if (content.HasProperty(propertyTypeAlias))
                {
                    var propertyValue = property.Value;

                    var propertyType = contentType.PropertyTypes.FirstOrDefault(pt => pt.Alias == propertyTypeAlias);

                    //TODO: It would be heaps nicer if we didn't have to hard code references to specific property editors
                    // we'd have to modify the packaging format to denote how to parse/store the value instead of relying on this

                    if (propertyType != null)
                    {
                        if (propertyType.PropertyEditorAlias == Constants.PropertyEditors.CheckBoxListAlias)
                        {

                            //TODO: We need to refactor this so the packager isn't making direct db calls for an 'edge' case
                            var database = ApplicationContext.Current.DatabaseContext.Database;
                            var dtos = database.Fetch<DataTypePreValueDto>("WHERE datatypeNodeId = @Id", new { Id = propertyType.DataTypeDefinitionId });

                            var propertyValueList = new List<string>();
                            foreach (var preValue in propertyValue.Split(','))
                            {
                                propertyValueList.Add(dtos.Single(x => x.Value == preValue).Id.ToString(CultureInfo.InvariantCulture));
                            }

                            propertyValue = string.Join(",", propertyValueList.ToArray());

                        }
                    }
                    //set property value
                    content.SetValue(propertyTypeAlias, propertyValue);
                }
            }

            return content;
        }

        #endregion

        #region ContentTypes

        /// <summary>
        /// Exports an <see cref="IContentType"/> to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="contentType">ContentType to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ContentType item.</returns>
        public XElement Export(IContentType contentType, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ExportingContentType.IsRaisedEventCancelled(new ExportEventArgs<IContentType>(contentType, "DocumentType"), this))
                    return new XElement("DocumentType");
            }

            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(_dataTypeService, contentType);

            if (raiseEvents)
                ExportedContentType.RaiseEvent(new ExportEventArgs<IContentType>(contentType, xml, false), this);

            return xml;
        }

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
            var fields = new List<TopologicalSorter.DependencyField<XElement>>();
            var isSingleDocTypeImport = unsortedDocumentTypes.Count == 1;
            if (isSingleDocTypeImport == false)
            {
                //NOTE Here we sort the doctype XElements based on dependencies
                //before creating the doc types - this should also allow for a better structure/inheritance support.
                foreach (var documentType in unsortedDocumentTypes)
                {
                    var elementCopy = documentType;
                    var infoElement = elementCopy.Element("Info");
                    var dependencies = new List<string>();

                    //Add the Master as a dependency
                    if (elementCopy.Element("Master") != null &&
                        string.IsNullOrEmpty(elementCopy.Element("Master").Value) == false)
                    {
                        dependencies.Add(elementCopy.Element("Master").Value);
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

                    var field = new TopologicalSorter.DependencyField<XElement>
                    {
                        Alias = infoElement.Element("Alias").Value,
                        Item = new Lazy<XElement>(() => elementCopy),
                        DependsOn = dependencies.ToArray()
                    };

                    fields.Add(field);
                }
            }

            //Sorting the Document Types based on dependencies - if its not a single doc type import ref. #U4-5921
            var documentTypes = isSingleDocTypeImport
                ? unsortedDocumentTypes.ToList()
                : TopologicalSorter.GetSortedItems(fields).ToList();

            //Iterate the sorted document types and create them as IContentType objects
            foreach (var documentType in documentTypes)
            {
                var alias = documentType.Element("Info").Element("Alias").Value;
                if (_importedContentTypes.ContainsKey(alias) == false)
                {
                    var contentType = _contentTypeService.GetContentType(alias);
                    _importedContentTypes.Add(alias, contentType == null
                                                         ? CreateContentTypeFromXml(documentType)
                                                         : UpdateContentTypeFromXml(documentType, contentType));
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
                             : _contentTypeService.GetContentType(masterAlias);
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
                    : _contentTypeService.GetContentType(masterAlias);

                contentType.SetLazyParentId(new Lazy<int>(() => parent.Id));
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
                            : _contentTypeService.GetContentType(compositionAlias);
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
                        _logger.Warn<PackagingService>(
                            string.Format(
                                "Packager: Error handling allowed templates. Template with alias '{0}' could not be found.",
                                alias));
                    }
                }

                contentType.AllowedTemplates = allowedTemplates;
            }

            if (string.IsNullOrEmpty(defaultTemplateElement.Value) == false)
            {
                var defaultTemplate = _fileService.GetTemplate(defaultTemplateElement.Value.ToSafeAlias());
                if (defaultTemplate != null)
                {
                    contentType.SetDefaultTemplate(defaultTemplate);
                }
                else
                {
                    _logger.Warn<PackagingService>(
                        string.Format(
                            "Packager: Error handling default template. Default template with alias '{0}' could not be found.",
                            defaultTemplateElement.Value));
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

                var dataTypeDefinition = _dataTypeService.GetDataTypeDefinitionById(dataTypeDefinitionId);

                //If no DataTypeDefinition with the guid from the xml wasn't found OR the ControlId on the DataTypeDefinition didn't match the DataType Id
                //We look up a DataTypeDefinition that matches

                //we'll check if it is a GUID (legacy id for a property editor)
                var legacyPropertyEditorId = Guid.Empty;
                Guid.TryParse(property.Element("Type").Value, out legacyPropertyEditorId);
                //get the alias as a string for use below
                var propertyEditorAlias = property.Element("Type").Value.Trim();

                //If no DataTypeDefinition with the guid from the xml wasn't found OR the ControlId on the DataTypeDefinition didn't match the DataType Id
                //We look up a DataTypeDefinition that matches

                if (dataTypeDefinition == null)
                {
                    var dataTypeDefinitions = legacyPropertyEditorId != Guid.Empty
                                                  ? _dataTypeService.GetDataTypeDefinitionByControlId(legacyPropertyEditorId)
                                                  : _dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(propertyEditorAlias);
                    if (dataTypeDefinitions != null && dataTypeDefinitions.Any())
                    {
                        dataTypeDefinition = dataTypeDefinitions.First();
                    }
                }
                else if (legacyPropertyEditorId != Guid.Empty && dataTypeDefinition.ControlId != legacyPropertyEditorId)
                {
                    var dataTypeDefinitions = _dataTypeService.GetDataTypeDefinitionByControlId(legacyPropertyEditorId);
                    if (dataTypeDefinitions != null && dataTypeDefinitions.Any())
                    {
                        dataTypeDefinition = dataTypeDefinitions.First();
                    }
                }
                else if (dataTypeDefinition.PropertyEditorAlias != propertyEditorAlias)
                {
                    var dataTypeDefinitions = _dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(propertyEditorAlias);
                    if (dataTypeDefinitions != null && dataTypeDefinitions.Any())
                    {
                        dataTypeDefinition = dataTypeDefinitions.First();
                    }
                }

                // For backwards compatibility, if no datatype with that ID can be found, we're letting this fail silently.
                // This means that the property will not be created.
                if (dataTypeDefinition == null)
                {
                    _logger.Warn<PackagingService>(
                        string.Format("Packager: Error handling creation of PropertyType '{0}'. Could not find DataTypeDefintion with unique id '{1}' nor one referencing the DataType with a property editor alias (or legacy control id) '{2}'. Did the package creator forget to package up custom datatypes? This property will be converted to a label/readonly editor if one exists.",
                                      property.Element("Name").Value,
                                      dataTypeDefinitionId,
                                      property.Element("Type").Value.Trim()));

                    //convert to a label!
                    dataTypeDefinition = _dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(Constants.PropertyEditors.NoEditAlias).FirstOrDefault();
                    //if for some odd reason this isn't there then ignore
                    if (dataTypeDefinition == null) continue;
                }

                var propertyType = new PropertyType(dataTypeDefinition, property.Element("Alias").Value)
                                       {
                                           Name = property.Element("Name").Value,
                                           Description = property.Element("Description").Value,
                                           Mandatory = property.Element("Mandatory").Value.ToLowerInvariant().Equals("true"),
                                           ValidationRegExp = property.Element("Validation").Value
                                       };

                var tab = property.Element("Tab").Value;
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
                if (_importedContentTypes.ContainsKey(alias))
                {
                    var allowedChild = _importedContentTypes[alias];
                    if (allowedChild == null || allowedChildren.Any(x => x.Id.IsValueCreated && x.Id.Value == allowedChild.Id)) continue;

                    allowedChildren.Add(new ContentTypeSort(new Lazy<int>(() => allowedChild.Id), sortOrder, allowedChild.Alias));
                    sortOrder++;
                }
                else
                {
                    _logger.Warn<PackagingService>(
                    string.Format(
                        "Packager: Error handling DocumentType structure. DocumentType with alias '{0}' could not be found and was not added to the structure for '{1}'.",
                        alias, contentType.Alias));
                }
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
            using (var repository = _repositoryFactory.CreateContentTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContentType>.Builder.Where(x => x.Alias == contentTypeAlias);
                var types = repository.GetByQuery(query);

                if (!types.Any())
                    throw new Exception(
                        string.Format("No ContentType matching the passed in Alias: '{0}' was found",
                                      contentTypeAlias));

                var contentType = types.First();

                if (contentType == null)
                    throw new Exception(string.Format("ContentType matching the passed in Alias: '{0}' was null",
                                                      contentTypeAlias));

                return contentType;
            }
        }

        #endregion

        #region DataTypes

        /// <summary>
        /// Exports a list of Data Types
        /// </summary>
        /// <param name="dataTypeDefinitions">List of data types to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDataTypeDefinition objects</returns>
        public XElement Export(IEnumerable<IDataTypeDefinition> dataTypeDefinitions, bool raiseEvents = true)
        {
            var container = new XElement("DataTypes");
            foreach (var dataTypeDefinition in dataTypeDefinitions)
            {
                container.Add(Export(dataTypeDefinition, raiseEvents));
            }
            return container;
        }

        /// <summary>
        /// Exports a single Data Type
        /// </summary>
        /// <param name="dataTypeDefinition">Data type to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDataTypeDefinition object</returns>
        public XElement Export(IDataTypeDefinition dataTypeDefinition, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ExportingDataType.IsRaisedEventCancelled(new ExportEventArgs<IDataTypeDefinition>(dataTypeDefinition, "DataType"), this))
                    return new XElement("DataType");
            }

            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(_dataTypeService, dataTypeDefinition);

            if (raiseEvents)
                ExportedDataType.RaiseEvent(new ExportEventArgs<IDataTypeDefinition>(dataTypeDefinition, xml, false), this);

            return xml;
        }

        /// <summary>
        /// Imports and saves package xml as <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the user</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated DataTypeDefinitions</returns>
        public IEnumerable<IDataTypeDefinition> ImportDataTypeDefinitions(XElement element, int userId = 0, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ImportingDataType.IsRaisedEventCancelled(new ImportEventArgs<IDataTypeDefinition>(element), this))
                    return Enumerable.Empty<IDataTypeDefinition>();
            }

            var name = element.Name.LocalName;
            if (name.Equals("DataTypes") == false && name.Equals("DataType") == false)
            {
                throw new ArgumentException("The passed in XElement is not valid! It does not contain a root element called 'DataTypes' for multiple imports or 'DataType' for a single import.");
            }

            var dataTypes = new Dictionary<string, IDataTypeDefinition>();
            var dataTypeElements = name.Equals("DataTypes")
                                       ? (from doc in element.Elements("DataType") select doc).ToList()
                                       : new List<XElement> { element };

            foreach (var dataTypeElement in dataTypeElements)
            {
                var dataTypeDefinitionName = dataTypeElement.Attribute("Name").Value;

                var legacyPropertyEditorId = Guid.Empty;
                Guid.TryParse(dataTypeElement.Attribute("Id").Value, out legacyPropertyEditorId);

                var dataTypeDefinitionId = new Guid(dataTypeElement.Attribute("Definition").Value);
                var databaseTypeAttribute = dataTypeElement.Attribute("DatabaseType");

                var definition = _dataTypeService.GetDataTypeDefinitionById(dataTypeDefinitionId);
                //If the datatypedefinition doesn't already exist we create a new new according to the one in the package xml
                if (definition == null)
                {
                    var databaseType = databaseTypeAttribute != null
                                           ? databaseTypeAttribute.Value.EnumParse<DataTypeDatabaseType>(true)
                                           : DataTypeDatabaseType.Ntext;

                    //check if the Id was a GUID, that means it is referenced using the legacy property editor GUID id
                    if (legacyPropertyEditorId != Guid.Empty)
                    {
                        var dataTypeDefinition = new DataTypeDefinition(-1, legacyPropertyEditorId)
                            {
                                Key = dataTypeDefinitionId,
                                Name = dataTypeDefinitionName,
                                DatabaseType = databaseType
                            };
                        dataTypes.Add(dataTypeDefinitionName, dataTypeDefinition);
                    }
                    else
                    {
                        //the Id field is actually the string property editor Alias
                        var dataTypeDefinition = new DataTypeDefinition(-1, dataTypeElement.Attribute("Id").Value.Trim())
                        {
                            Key = dataTypeDefinitionId,
                            Name = dataTypeDefinitionName,
                            DatabaseType = databaseType
                        };
                        dataTypes.Add(dataTypeDefinitionName, dataTypeDefinition);
                    }

                }
            }

            var list = dataTypes.Select(x => x.Value).ToList();
            if (list.Any())
            {
                //NOTE: As long as we have to deal with the two types of PreValue lists (with/without Keys)
                //this is a bit of a pain to handle while ensuring that the imported DataTypes has PreValues
                //place when triggering the save event.

                _dataTypeService.Save(list, userId, false);//Save without raising events

                SavePrevaluesFromXml(list, dataTypeElements);//Save the PreValues for the current list of DataTypes

                _dataTypeService.Save(list, userId, true);//Re-save and raise events
            }

            if (raiseEvents)
                ImportedDataType.RaiseEvent(new ImportEventArgs<IDataTypeDefinition>(list, element, false), this);

            return list;
        }

        private void SavePrevaluesFromXml(List<IDataTypeDefinition> dataTypes, IEnumerable<XElement> dataTypeElements)
        {
            foreach (var dataTypeElement in dataTypeElements)
            {
                var prevaluesElement = dataTypeElement.Element("PreValues");
                if (prevaluesElement == null) continue;

                var dataTypeDefinitionName = dataTypeElement.Attribute("Name").Value;
                var dataTypeDefinition = dataTypes.First(x => x.Name == dataTypeDefinitionName);

                var valuesWithoutKeys = prevaluesElement.Elements("PreValue")
                                                        .Where(x => ((string)x.Attribute("Alias")).IsNullOrWhiteSpace())
                                                        .Select(x => x.Attribute("Value").Value);

                var valuesWithKeys = prevaluesElement.Elements("PreValue")
                    .Where(x => ((string)x.Attribute("Alias")).IsNullOrWhiteSpace() == false)
                    .ToDictionary(
                        key => (string)key.Attribute("Alias"),
                        val => new PreValue((string)val.Attribute("Value")));

                //save the values with keys
                _dataTypeService.SavePreValues(dataTypeDefinition, valuesWithKeys);

                //save the values without keys (this is legacy)
                _dataTypeService.SavePreValues(dataTypeDefinition.Id, valuesWithoutKeys);
            }
        }

        #endregion

        #region Dictionary Items

        /// <summary>
        /// Exports a list of <see cref="IDictionaryItem"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dictionaryItem">List of dictionary items to export</param>
        /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDictionaryItem objects</returns>
        public XElement Export(IEnumerable<IDictionaryItem> dictionaryItem, bool includeChildren = true, bool raiseEvents = true)
        {
            var xml = new XElement("DictionaryItems");
            foreach (var item in dictionaryItem)
            {
                xml.Add(Export(item, includeChildren, raiseEvents));
            }
            return xml;
        }

        /// <summary>
        /// Exports a single <see cref="IDictionaryItem"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dictionaryItem">Dictionary Item to export</param>
        /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDictionaryItem object</returns>
        public XElement Export(IDictionaryItem dictionaryItem, bool includeChildren, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ExportingDictionaryItem.IsRaisedEventCancelled(new ExportEventArgs<IDictionaryItem>(dictionaryItem, "DictionaryItem"), this))
                    return new XElement("DictionaryItem");
            }

            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(dictionaryItem);

            if (includeChildren)
            {
                var children = _localizationService.GetDictionaryItemChildren(dictionaryItem.Key);
                foreach (var child in children)
                {
                    xml.Add(Export(child, true));
                }
            }

            if (raiseEvents)
                ExportedDictionaryItem.RaiseEvent(new ExportEventArgs<IDictionaryItem>(dictionaryItem, xml, false), this);

            return xml;
        }

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
        /// Exports a list of <see cref="ILanguage"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="languages">List of Languages to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ILanguage objects</returns>
        public XElement Export(IEnumerable<ILanguage> languages, bool raiseEvents = true)
        {
            var xml = new XElement("Languages");
            foreach (var language in languages)
            {
                xml.Add(Export(language, raiseEvents));
            }
            return xml;
        }

        /// <summary>
        /// Exports a single <see cref="ILanguage"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="language">Language to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ILanguage object</returns>
        public XElement Export(ILanguage language, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ExportingLanguage.IsRaisedEventCancelled(new ExportEventArgs<ILanguage>(language, "Language"), this))
                    return new XElement("Language");
            }

            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(language);

            if (raiseEvents)
                ExportedLanguage.RaiseEvent(new ExportEventArgs<ILanguage>(language, xml, false), this);

            return xml;
        }

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
            var controlType = macroElement.Element("scriptType").Value;
            var controlAssembly = macroElement.Element("scriptAssembly").Value;
            var xsltPath = macroElement.Element("xslt").Value;
            var scriptPath = macroElement.Element("scriptingFile").Value;

            //Following xml elements are treated as nullable properties
            var useInEditorElement = macroElement.Element("useInEditor");
            var useInEditor = false;
            if (useInEditorElement != null && string.IsNullOrEmpty(useInEditorElement.Value) == false)
            {
                useInEditor = bool.Parse(useInEditorElement.Value);
            }
            var cacheDurationElement = macroElement.Element("refreshRate");
            var cacheDuration = 0;
            if (cacheDurationElement != null && string.IsNullOrEmpty(cacheDurationElement.Value) == false)
            {
                cacheDuration = int.Parse(cacheDurationElement.Value);
            }
            var cacheByMemberElement = macroElement.Element("cacheByMember");
            var cacheByMember = false;
            if (cacheByMemberElement != null && string.IsNullOrEmpty(cacheByMemberElement.Value) == false)
            {
                cacheByMember = bool.Parse(cacheByMemberElement.Value);
            }
            var cacheByPageElement = macroElement.Element("cacheByPage");
            var cacheByPage = false;
            if (cacheByPageElement != null && string.IsNullOrEmpty(cacheByPageElement.Value) == false)
            {
                cacheByPage = bool.Parse(cacheByPageElement.Value);
            }
            var dontRenderElement = macroElement.Element("dontRender");
            var dontRender = true;
            if (dontRenderElement != null && string.IsNullOrEmpty(dontRenderElement.Value) == false)
            {
                dontRender = bool.Parse(dontRenderElement.Value);
            }

            var existingMacro = _macroService.GetByAlias(macroAlias) as Macro;
            var macro = existingMacro ?? new Macro(macroAlias, macroName, controlType, controlAssembly, xsltPath, scriptPath,
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

                    if (macro.Properties.Any(x => x.Alias == propertyAlias)) continue;
                    macro.Properties.Add(new MacroProperty(propertyAlias, propertyName, sortOrder, editorAlias));
                    sortOrder++;
                }
            }
            return macro;
        }

        /// <summary>
        /// Exports a list of <see cref="IMacro"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="macros">Macros to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IMacro objects</returns>
        public XElement Export(IEnumerable<IMacro> macros, bool raiseEvents = true)
        {
            var xml = new XElement("Macros");
            foreach (var item in macros)
            {
                xml.Add(Export(item, raiseEvents));
            }
            return xml;
        }

        /// <summary>
        /// Exports a single <see cref="IMacro"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="macro">Macro to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IMacro object</returns>
        public XElement Export(IMacro macro, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ExportingMacro.IsRaisedEventCancelled(new ExportEventArgs<IMacro>(macro, "macro"), this))
                    return new XElement("macro");
            }

            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(macro);

            if (raiseEvents)
                ExportedMacro.RaiseEvent(new ExportEventArgs<IMacro>(macro, xml, false), this);

            return xml;
        }

        #endregion

        #region Members

        /// <summary>
        /// Exports an <see cref="IMedia"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="member">Member to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Member object</returns>
        public XElement Export(IMember member)
        {
            var exporter = new EntityXmlSerializer();
            return exporter.Serialize(_dataTypeService, member);
        }

        #endregion

        #region Media

        /// <summary>
        /// Exports an <see cref="IMedia"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="media">Media to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Media object</returns>
        public XElement Export(IMedia media, bool deep = false, bool raiseEvents = true)
        {
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : media.ContentType.Alias.ToSafeAliasWithForcingCheck();

            if (raiseEvents)
            {
                if (ExportingMedia.IsRaisedEventCancelled(new ExportEventArgs<IMedia>(media, nodeName), this))
                    return new XElement(nodeName);
            }

            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(_mediaService, _dataTypeService, _userService, media, deep);

            if (raiseEvents)
                ExportedMedia.RaiseEvent(new ExportEventArgs<IMedia>(media, xml, false), this);

            return xml;
        }


        #endregion

        #region MediaTypes

        /// <summary>
        /// Exports an <see cref="IMediaType"/> to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="mediaType">MediaType to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the MediaType item.</returns>
        internal XElement Export(IMediaType mediaType)
        {
            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(_dataTypeService, mediaType);

            return xml;
        }

        #endregion

        #region Package Manifest
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

            var fields = new List<TopologicalSorter.DependencyField<XElement>>();
            foreach (XElement tempElement in templateElements)
            {
                var dependencies = new List<string>();
                var elementCopy = tempElement;
                //Ensure that the Master of the current template is part of the import, otherwise we ignore this dependency as part of the dependency sorting.
                if (elementCopy.Element("Master") != null &&
                    string.IsNullOrEmpty(elementCopy.Element("Master").Value) == false &&
                    templateElements.Any(x => x.Element("Alias").Value == elementCopy.Element("Master").Value))
                {
                    dependencies.Add(elementCopy.Element("Master").Value);
                }
                else if (elementCopy.Element("Master") != null &&
                         string.IsNullOrEmpty(elementCopy.Element("Master").Value) == false &&
                         templateElements.Any(x => x.Element("Alias").Value == elementCopy.Element("Master").Value) ==
                         false)
                {
                    _logger.Info<PackagingService>(string.Format("Template '{0}' has an invalid Master '{1}', so the reference has been ignored.", elementCopy.Element("Alias").Value, elementCopy.Element("Master").Value));
                }

                var field = new TopologicalSorter.DependencyField<XElement>
                                {
                                    Alias = elementCopy.Element("Alias").Value,
                                    Item = new Lazy<XElement>(() => elementCopy),
                                    DependsOn = dependencies.ToArray()
                                };

                fields.Add(field);
            }
            //Sort templates by dependencies to a potential master template
            var sortedElements = TopologicalSorter.GetSortedItems(fields);
            foreach (var templateElement in sortedElements)
            {
                var templateName = templateElement.Element("Name").Value;
                var alias = templateElement.Element("Alias").Value;
                var design = templateElement.Element("Design").Value;
                var masterElement = templateElement.Element("Master");

                var isMasterPage = IsMasterPageSyntax(design);
                var path = isMasterPage ? MasterpagePath(alias) : ViewPath(alias);

                var existingTemplate = _fileService.GetTemplate(alias) as Template;
                var template = existingTemplate ?? new Template(path, templateName, alias);
                template.Content = design;
                if (masterElement != null && string.IsNullOrEmpty(masterElement.Value) == false)
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

        /// <summary>
        /// Exports a list of <see cref="ITemplate"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="templates">List of Templates to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ITemplate objects</returns>
        public XElement Export(IEnumerable<ITemplate> templates, bool raiseEvents = true)
        {
            var xml = new XElement("Templates");
            foreach (var item in templates)
            {
                xml.Add(Export(item, raiseEvents));
            }
            return xml;
        }

        /// <summary>
        /// Exports a single <see cref="ITemplate"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="template">Template to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ITemplate object</returns>
        public XElement Export(ITemplate template, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (ExportingTemplate.IsRaisedEventCancelled(new ExportEventArgs<ITemplate>(template, "Template"), this))
                    return new XElement("Template");
            }

            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(template);

            if (raiseEvents)
                ExportedTemplate.RaiseEvent(new ExportEventArgs<ITemplate>(template, xml, false), this);

            return xml;
        }

        #endregion

        #region Stylesheets
        #endregion

        #region Installation

        internal IPackageInstallation PackageInstallation
        {
            private get { return _packageInstallation ?? new PackageInstallation(this, _macroService, _fileService, new PackageExtraction()); }
            set { _packageInstallation = value; }
        }

        internal InstallationSummary InstallPackage(string packageFilePath, int userId = 0, bool raiseEvents = false)
        {
            if (raiseEvents)
            {
                var metaData = GetPackageMetaData(packageFilePath);
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
                ImportedPackage.RaiseEvent(new ImportPackageEventArgs<InstallationSummary>(installationSummary, false), this);
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
        #endregion

        #region Event Handlers
        /// <summary>
        /// Occurs before Importing Content
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IContent>> ImportingContent;

        /// <summary>
        /// Occurs after Content is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IContent>> ImportedContent;


        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IContent>> ExportingContent;

        /// <summary>
        /// Occurs after Content is Exported to Xml
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IContent>> ExportedContent;

        /// <summary>
        /// Occurs before Exporting Media
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IMedia>> ExportingMedia;

        /// <summary>
        /// Occurs after Media is Exported to Xml
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IMedia>> ExportedMedia;

        /// <summary>
        /// Occurs before Importing ContentType
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IContentType>> ImportingContentType;

        /// <summary>
        /// Occurs after ContentType is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IContentType>> ImportedContentType;

        /// <summary>
        /// Occurs before Exporting ContentType
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IContentType>> ExportingContentType;

        /// <summary>
        /// Occurs after ContentType is Exported to Xml
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IContentType>> ExportedContentType;

        /// <summary>
        /// Occurs before Importing DataType
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IDataTypeDefinition>> ImportingDataType;

        /// <summary>
        /// Occurs after DataType is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IDataTypeDefinition>> ImportedDataType;

        /// <summary>
        /// Occurs before Exporting DataType
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IDataTypeDefinition>> ExportingDataType;

        /// <summary>
        /// Occurs after DataType is Exported to Xml
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IDataTypeDefinition>> ExportedDataType;

        /// <summary>
        /// Occurs before Importing DictionaryItem
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IDictionaryItem>> ImportingDictionaryItem;

        /// <summary>
        /// Occurs after DictionaryItem is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IDictionaryItem>> ImportedDictionaryItem;

        /// <summary>
        /// Occurs before Exporting DictionaryItem
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IDictionaryItem>> ExportingDictionaryItem;

        /// <summary>
        /// Occurs after DictionaryItem is Exported to Xml
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IDictionaryItem>> ExportedDictionaryItem;

        /// <summary>
        /// Occurs before Importing Macro
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IMacro>> ImportingMacro;

        /// <summary>
        /// Occurs after Macro is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<IMacro>> ImportedMacro;

        /// <summary>
        /// Occurs before Exporting Macro
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IMacro>> ExportingMacro;

        /// <summary>
        /// Occurs after Macro is Exported to Xml
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<IMacro>> ExportedMacro;

        /// <summary>
        /// Occurs before Importing Language
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<ILanguage>> ImportingLanguage;

        /// <summary>
        /// Occurs after Language is Imported and Saved
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportEventArgs<ILanguage>> ImportedLanguage;

        /// <summary>
        /// Occurs before Exporting Language
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<ILanguage>> ExportingLanguage;

        /// <summary>
        /// Occurs after Language is Exported to Xml
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<ILanguage>> ExportedLanguage;

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
        /// Occurs before Exporting Template
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<ITemplate>> ExportingTemplate;

        /// <summary>
        /// Occurs after Template is Exported to Xml
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ExportEventArgs<ITemplate>> ExportedTemplate;

        /// <summary>
        /// Occurs before Importing umbraco package
        /// </summary>
        internal static event TypedEventHandler<IPackagingService, ImportPackageEventArgs<string>> ImportingPackage;

        /// <summary>
        /// Occurs after a apckage is imported
        /// </summary>
        internal static event TypedEventHandler<IPackagingService, ImportPackageEventArgs<InstallationSummary>> ImportedPackage;

        #endregion
    }
}

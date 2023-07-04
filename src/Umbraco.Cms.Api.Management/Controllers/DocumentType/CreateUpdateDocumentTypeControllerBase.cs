using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using ContentTypeSort = Umbraco.Cms.Core.Models.ContentTypeSort;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

// FIXME: pretty much everything here should be moved to mappers and possibly new services for content type editing - like the ContentEditingService for content
public abstract class CreateUpdateDocumentTypeControllerBase : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemplateService _templateService;
    private readonly IEntityService _entityService;
    private const int MaxInheritance = 1;

    protected CreateUpdateDocumentTypeControllerBase(
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        ITemplateService templateService,
        IEntityService entityService)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
        _shortStringHelper = shortStringHelper;
        _templateService = templateService;
        _entityService = entityService;
    }

    protected async Task<ContentTypeOperationStatus> HandleRequest<TRequestModel, TPropertyType, TPropertyTypeContainer>(IContentType contentType, TRequestModel requestModel)
        where TRequestModel : ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>, IDocumentTypeRequestModel
        where TPropertyType : PropertyTypeModelBase
        where TPropertyTypeContainer : PropertyTypeContainerModelBase
    {
        // validate content type alias
        if (contentType.Alias.Equals(requestModel.Alias) is false)
        {
            if (_contentTypeService.GetAllContentTypeAliases().Contains(requestModel.Alias))
            {
                return ContentTypeOperationStatus.DuplicateAlias;
            }

            var reservedModelAliases = new[] { "system" };
            if (reservedModelAliases.InvariantContains(requestModel.Alias))
            {
                return ContentTypeOperationStatus.InvalidAlias;
            }
        }

        // validate properties
        var reservedPropertyTypeNames = typeof(IPublishedContent).GetProperties().Select(x => x.Name)
            .Union(typeof(IPublishedContent).GetMethods().Select(x => x.Name))
            .ToArray();
        foreach (TPropertyType propertyType in requestModel.Properties)
        {
            if (propertyType.Alias.Equals(requestModel.Alias, StringComparison.OrdinalIgnoreCase))
            {
                return ContentTypeOperationStatus.InvalidPropertyTypeAlias;
            }

            if (reservedPropertyTypeNames.InvariantContains(propertyType.Alias))
            {
                return ContentTypeOperationStatus.InvalidPropertyTypeAlias;
            }
        }

        // validate property data types exists.
        Guid[] dataTypeKeys = requestModel.Properties.Select(property => property.DataTypeId).ToArray();
        Dictionary<Guid, IDataType> dataTypesByKey = (await _dataTypeService.GetAllAsync(dataTypeKeys))
            .ToDictionary(x => x.Key);

        if (dataTypeKeys.Length != dataTypesByKey.Count)
        {
            return ContentTypeOperationStatus.InvalidDataType;
        }

        // Only one composition can be inherited, and the key of that composition must be the parent ID.
        ContentTypeComposition[] inheritedCompositions = requestModel
            .Compositions
            .Where(x => x.CompositionType is ContentTypeCompositionType.Inheritance)
            .ToArray();
        if (inheritedCompositions.Length > MaxInheritance || (inheritedCompositions.Any() && inheritedCompositions.First().Id != requestModel.ParentId))
        {
            return ContentTypeOperationStatus.InvalidInheritance;
        }

        // filter out properties and containers with no name/alias
        requestModel.Properties = requestModel.Properties.Where(propertyType => propertyType.Alias.IsNullOrWhiteSpace() is false).ToArray();
        requestModel.Containers = requestModel.Containers.Where(container => container.Name.IsNullOrWhiteSpace() is false).ToArray();

        int? parentId = null;
        if (requestModel.ParentId is not null)
        {
            Attempt<int> parentContentTypeIdAttempt = _entityService.GetId(requestModel.ParentId.Value, UmbracoObjectTypes.DocumentType);
            if (parentContentTypeIdAttempt.Success is false)
            {
                // Of course document document type container is a separate thing, so we have to try again if we can't find it >:(
                // We can probably do this smarter....

                // TODO: Make this not suck
                // Try container...
                Attempt<int> containerIdAttempt = _entityService.GetId(requestModel.ParentId.Value, UmbracoObjectTypes.DocumentTypeContainer);

                if (containerIdAttempt.Success is false)
                {
                    return ContentTypeOperationStatus.ParentNotFound;
                }

                parentId = containerIdAttempt.Result;
            }
            else
            {
                parentId = parentContentTypeIdAttempt.Result;
            }
        }

        // update basic content type settings
        contentType.ParentId = parentId ?? default;
        contentType.Alias = requestModel.Alias;
        contentType.Description = requestModel.Description;
        contentType.Icon = requestModel.Icon;
        contentType.IsElement = requestModel.IsElement;
        contentType.Name = requestModel.Name;
        contentType.AllowedAsRoot = requestModel.AllowedAsRoot;
        contentType.SetVariesBy(ContentVariation.Culture, requestModel.VariesByCulture);
        contentType.SetVariesBy(ContentVariation.Segment, requestModel.VariesBySegment);

        // update allowed content types
        var allowedContentTypesUnchanged = contentType.AllowedContentTypes?
            .OrderBy(contentTypeSort => contentTypeSort.SortOrder)
            .Select(contentTypeSort => contentTypeSort.Key)
            .SequenceEqual(requestModel.AllowedContentTypes
                .OrderBy(contentTypeSort => contentTypeSort.SortOrder)
                .Select(contentTypeSort => contentTypeSort.Id)) ?? false;
        if (allowedContentTypesUnchanged is false)
        {
            // need the content type IDs here - yet, anyway - see FIXME in Umbraco.Cms.Core.Models.ContentTypeSort
            var allContentTypesByKey = _contentTypeService.GetAll().ToDictionary(c => c.Key);
            contentType.AllowedContentTypes = requestModel
                .AllowedContentTypes
                .Select((contentTypeSort, index) => allContentTypesByKey.TryGetValue(contentTypeSort.Id, out IContentType? ct)
                    ? new ContentTypeSort(new Lazy<int>(() => ct.Id), contentTypeSort.Id, index, ct.Alias)
                    : null)
                .WhereNotNull()
                .ToArray();
        }

        // build a dictionary of parent container IDs and their names (we need it when mapping property groups)
        var parentContainerNamesById = requestModel
            .Containers
            .Where(container => container.ParentId is not null)
            .DistinctBy(container => container.ParentId)
            .ToDictionary(
                container => container.ParentId!.Value,
                container => requestModel.Containers.First(c => c.Id == container.ParentId).Name!);

        // FIXME: when refactoring for media and member types, this needs to be some kind of abstract implementation - media and member types do not support publishing
        const bool supportsPublishing = true;

        // update properties and groups
        PropertyGroup[] propertyGroups = requestModel.Containers.Select(container =>
            {
                PropertyGroup propertyGroup = contentType.PropertyGroups.FirstOrDefault(group => group.Key == container.Id) ??
                                              new PropertyGroup(supportsPublishing);
                // NOTE: eventually group.Type should be a string to make the client more flexible; for now we'll have to parse the string value back to its expected enum
                propertyGroup.Type = Enum.Parse<PropertyGroupType>(container.Type);
                propertyGroup.Name = container.Name;
                // this is not pretty, but this is how the data structure is at the moment; we just have to live with it for the time being.
                var alias = container.Name!;
                if (container.ParentId is not null)
                {
                    alias = $"{parentContainerNamesById[container.ParentId.Value]}/{alias}";
                }
                propertyGroup.Alias = alias;
                propertyGroup.SortOrder = container.SortOrder;

                IPropertyType[] properties = requestModel
                    .Properties
                    .Where(property => property.ContainerId == container.Id)
                    .Select(property =>
                    {
                        // get the selected data type
                        // NOTE: this only works because we already ensured that the data type is present in the dataTypesByKey dictionary
                        IDataType dataType = dataTypesByKey[property.DataTypeId];

                        // get the current property type (if it exists)
                        IPropertyType propertyType = contentType.PropertyTypes.FirstOrDefault(pt => pt.Key == property.Id)
                                                     ?? new Core.Models.PropertyType(_shortStringHelper, dataType);

                        propertyType.Name = property.Name;
                        propertyType.DataTypeId = dataType.Id;
                        propertyType.DataTypeKey = dataType.Key;
                        propertyType.Mandatory = property.Validation.Mandatory;
                        propertyType.MandatoryMessage = property.Validation.MandatoryMessage;
                        propertyType.ValidationRegExp = property.Validation.RegEx;
                        propertyType.ValidationRegExpMessage = property.Validation.RegExMessage;
                        propertyType.SetVariesBy(ContentVariation.Culture, property.VariesByCulture);
                        propertyType.SetVariesBy(ContentVariation.Segment, property.VariesBySegment);
                        propertyType.PropertyGroupId = new Lazy<int>(() => propertyGroup.Id, false);
                        propertyType.Alias = property.Alias;
                        propertyType.Description = property.Description;
                        propertyType.SortOrder = property.SortOrder;
                        propertyType.LabelOnTop = property.Appearance.LabelOnTop;

                        return propertyType;
                    })
                    .ToArray();

                if (properties.Any() is false && parentContainerNamesById.ContainsKey(container.Id) is false)
                {
                    // FIXME: if at all possible, retain empty containers (bad DX to remove stuff that's been attempted saved)
                    return null;
                }

                if (propertyGroup.PropertyTypes == null || propertyGroup.PropertyTypes.SequenceEqual(properties) is false)
                {
                    propertyGroup.PropertyTypes = new PropertyTypeCollection(supportsPublishing, properties);
                }

                return propertyGroup;
            })
            .WhereNotNull()
            .ToArray();

        // Handle orphaned properties
        IEnumerable<TPropertyType> orphanedPropertyTypeModels = requestModel.Properties.Where(x => x.ContainerId is null);

        if(orphanedPropertyTypeModels.Any())
        {
            var orphanedProperties = new List<IPropertyType>();
            foreach (TPropertyType propertyTypeModel in orphanedPropertyTypeModels)
            {
                // TODO: Don't duplicate the code above
                IDataType dataType = dataTypesByKey[propertyTypeModel.DataTypeId];

                IPropertyType existing = contentType.PropertyTypes.FirstOrDefault(pt => pt.Key == propertyTypeModel.Id)
                                         ?? new Core.Models.PropertyType(_shortStringHelper, dataType);
                existing.Name = propertyTypeModel.Name;
                existing.DataTypeId = dataType.Id;
                existing.DataTypeKey = dataType.Key;
                existing.Mandatory = propertyTypeModel.Validation.Mandatory;
                existing.MandatoryMessage = propertyTypeModel.Validation.MandatoryMessage;
                existing.ValidationRegExp = propertyTypeModel.Validation.RegEx;
                existing.ValidationRegExpMessage = propertyTypeModel.Validation.RegExMessage;
                existing.SetVariesBy(ContentVariation.Culture, propertyTypeModel.VariesByCulture);
                existing.SetVariesBy(ContentVariation.Segment, propertyTypeModel.VariesBySegment);
                // existing.PropertyGroupId = new Lazy<int>(() => propertyGroup.Id, false);
                existing.Alias = propertyTypeModel.Alias;
                existing.Description = propertyTypeModel.Description;
                existing.SortOrder = propertyTypeModel.SortOrder;
                existing.LabelOnTop = propertyTypeModel.Appearance.LabelOnTop;

                orphanedProperties.Add(existing);
            }

            contentType.NoGroupPropertyTypes = new PropertyTypeCollection(supportsPublishing, orphanedProperties);
        }

        if (contentType.PropertyGroups.SequenceEqual(propertyGroups) is false)
        {
            contentType.PropertyGroups = new PropertyGroupCollection(propertyGroups);
        }

        // FIXME: handle properties outside containers ("generic properties") if they still exist
        // Updates compositions
        // We don't actually have to worry about alias collision here because that's also checked in the service
        // We'll probably want to refactor this to be able to return a proper ContentTypeOperationStatus.
        // In the mapping step here we only really care about the most immediate ancestors.
        // We only really have to care about removing when updating
        Guid[] currentKeys = contentType.ContentTypeComposition.Select(x => x.Key).ToArray();
        Guid[] targetCompositionKeys = requestModel.Compositions.Select(x => x.Id).ToArray();

        // We want to remove all of those that are in current, but not in targetCompositionKeys
        Guid[] remove = currentKeys.Except(targetCompositionKeys).ToArray();
        IEnumerable<Guid> add = targetCompositionKeys.Except(currentKeys).ToArray();

        foreach (Guid key in remove)
        {
            contentType.RemoveContentType(key);
        }

        // We have to look up the content types we want to add to composition, since we keep a full reference.
        // TODO: Make Async
        if (add.Any())
        {
            IContentType[] contentTypesToAdd = _contentTypeService.GetAll(add).ToArray();
            foreach (IContentType contentTypeToAdd in contentTypesToAdd)
            {
                contentType.AddContentType(contentTypeToAdd);
            }

        }

        // We need to handle the parent as well
        // We've already validated that there is only one
        ContentTypeComposition? parent = requestModel.Compositions
            .FirstOrDefault(x => x.CompositionType is ContentTypeCompositionType.Inheritance);
        if(parent is not null)
        {
            IContentType? parentType = await _contentTypeService.GetAsync(parent.Id);
            contentType.SetParent(parentType);
        }

        // update content type history clean-up
        contentType.HistoryCleanup ??= new HistoryCleanup();
        contentType.HistoryCleanup.PreventCleanup = requestModel.Cleanup.PreventCleanup;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = requestModel.Cleanup.KeepAllVersionsNewerThanDays;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = requestModel.Cleanup.KeepLatestVersionPerDayForDays;

        // update allowed templates and assign default template
        ITemplate[] allowedTemplates = requestModel.AllowedTemplateIds
            .Select(async templateId => await _templateService.GetAsync(templateId))
            .Select(t => t.Result)
            .WhereNotNull()
            .ToArray();
        contentType.AllowedTemplates = allowedTemplates;
        // NOTE: incidentally this also covers removing the default template; when requestModel.DefaultTemplateId is null,
        //       contentType.SetDefaultTemplate() will be called with a null value, which will reset the default template.
        contentType.SetDefaultTemplate(allowedTemplates.FirstOrDefault(t => t.Key == requestModel.DefaultTemplateId));

        // Validate that the all the compositions are allowed
        // Would be nice to maybe have this in a little nicer way, but for now it should be okay.
        IContentTypeComposition[] allContentTypes = _contentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();

        // TODO: Handle Update (should come pretty natty when we refactor this)
        if (contentType.Id == 0)
        {
            IEnumerable<Guid> allowedCompositionKeys =
                // NOTE: Here if we're checking for create we should pass null, otherwise the updated content type.
                _contentTypeService.GetAvailableCompositeContentTypes(null, allContentTypes, isElement: true)
                    .Results
                    .Where(x => x.Allowed)
                    .Select(x => x.Composition.Key);

            if (contentType.CompositionKeys().Except(allowedCompositionKeys).Any())
            {
                // We have a composition key that's not in the allowed composition keys
                return ContentTypeOperationStatus.InvalidComposition;
            }
        }


        // save content type
        // FIXME: create and use an async get method here.
        _contentTypeService.Save(contentType);

        return ContentTypeOperationStatus.Success;
    }
}

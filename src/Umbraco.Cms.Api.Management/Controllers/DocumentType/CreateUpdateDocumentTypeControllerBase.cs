using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
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

    protected CreateUpdateDocumentTypeControllerBase(IContentTypeService contentTypeService, IDataTypeService dataTypeService, IShortStringHelper shortStringHelper, ITemplateService templateService)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
        _shortStringHelper = shortStringHelper;
        _templateService = templateService;
    }

    protected ContentTypeOperationStatus HandleRequest<TRequestModel, TPropertyType, TPropertyTypeContainer>(IContentType contentType, TRequestModel requestModel)
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

        // validate property data types
        Guid[] dataTypeKeys = requestModel.Properties.Select(property => property.DataTypeId).ToArray();
        var dataTypesByKey = dataTypeKeys
            // FIXME: create GetAllAsync(params Guid[] keys) method on IDataTypeService
            .Select(async key => await _dataTypeService.GetAsync(key))
            .Select(t => t.Result)
            .WhereNotNull()
            .ToDictionary(dataType => dataType.Key);
        if (dataTypeKeys.Length != dataTypesByKey.Count())
        {
            return ContentTypeOperationStatus.InvalidDataType;
        }

        // filter out properties and containers with no name/alias
        requestModel.Properties = requestModel.Properties.Where(propertyType => propertyType.Alias.IsNullOrWhiteSpace() is false).ToArray();
        requestModel.Containers = requestModel.Containers.Where(container => container.Name.IsNullOrWhiteSpace() is false).ToArray();

        // update basic content type settings
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

        if (contentType.PropertyGroups.SequenceEqual(propertyGroups) is false)
        {
            contentType.PropertyGroups = new PropertyGroupCollection(propertyGroups);
        }

        // FIXME: handle properties outside containers ("generic properties") if they still exist
        // FIXME: handle compositions (yeah, that)

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

        // save content type
        // FIXME: create and use an async get method here.
        _contentTypeService.Save(contentType);

        return ContentTypeOperationStatus.Success;
    }
}

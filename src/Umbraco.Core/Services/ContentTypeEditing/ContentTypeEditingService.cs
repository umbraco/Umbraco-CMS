﻿using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public class ContentTypeEditingService : IContentTypeEditingService
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IEntityService _entityService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemplateService _templateService;
    private const int MaxInheritance = 1;

    public ContentTypeEditingService(
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper,
        ITemplateService templateService)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
        _entityService = entityService;
        _shortStringHelper = shortStringHelper;
        _templateService = templateService;
    }

    public async Task<Attempt<IContentType?, ContentTypeOperationStatus>> CreateAsync(ContentTypeCreateModel model, Guid performingUserId)
    {
        // Ensure no duplicate alias across documents, members, and media. Since this would break ModelsBuilder/published cache.
        // This this method gets aliases across documents, members, and media, so it covers it all
        if (_contentTypeService.GetAllContentTypeAliases().Contains(model.Alias))
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.DuplicateAlias, null);
        }

        var reservedModelAliases = new[] { "system" };
        if (reservedModelAliases.InvariantContains(model.Alias))
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidAlias, null);
        }

        // Validate properties for reserved names.
        // Because of models builder you cannot have an alias that already exists in IPublishedContent, for instance Path.
        // Since MyModel.Path would conflict with IPublishedContent.Path.
        var reservedPropertyTypeNames = typeof(IPublishedContent).GetProperties().Select(x => x.Name)
            .Union(typeof(IPublishedContent).GetMethods().Select(x => x.Name))
            .ToArray();
        foreach (ContentTypePropertyTypeModel propertyType in model.Properties)
        {
            if (propertyType.Alias.Equals(model.Alias, StringComparison.OrdinalIgnoreCase)
                || reservedPropertyTypeNames.InvariantContains(propertyType.Alias))
            {
                return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidPropertyTypeAlias, null);
            }
        }

        // validate property data types exists.
        Guid[] dataTypeKeys = model.Properties.Select(property => property.DataTypeKey).ToArray();
        var dataTypesByKey = (await _dataTypeService.GetAllAsync(dataTypeKeys)).ToDictionary(x => x.Key);
        if (dataTypeKeys.Length != dataTypesByKey.Count)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidDataType, null);
        }

        Guid[] KeysForCompositionType(CompositionType compositionType) => model
            .Compositions
            .Where(c => c.CompositionType == compositionType)
            .Select(c => c.Key)
            .ToArray();
        Guid[] inheritedKeys = KeysForCompositionType(CompositionType.Inheritance);
        Guid[] compositionKeys = KeysForCompositionType(CompositionType.Composition);

        // Only one composition can be of type inheritance, and composed items cannot also be inherited.
        if (inheritedKeys.Length > MaxInheritance || compositionKeys.Intersect(inheritedKeys).Any())
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidInheritance, null);
        }

        // a content type cannot be created/saved in an entity container (a folder) if has an inheritance type composition
        if (inheritedKeys.Any() && model.ParentKey.HasValue)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidParent, null);
        }

        // Validate that the all the compositions are allowed
        IContentType[] allContentTypes = _contentTypeService.GetAll().ToArray();
        IContentTypeComposition[] allContentTypesCompositions = allContentTypes.OfType<IContentTypeComposition>().ToArray();

        IEnumerable<Guid> allowedCompositionKeys =
            // NOTE: Here if we're checking for create we should pass null, otherwise the updated content type.
            _contentTypeService.GetAvailableCompositeContentTypes(null, allContentTypesCompositions, isElement: model.IsElement)
                .Results
                .Where(x => x.Allowed)
                .Select(x => x.Composition.Key);

        // Both inheritance and compositions.
        Guid[] allCompositionKeys = inheritedKeys.Union(compositionKeys).ToArray();
        IContentTypeComposition[] allCompositionTypes = allContentTypesCompositions.Where(x => allCompositionKeys.Contains(x.Key)).ToArray();

        if (allCompositionKeys.Length != allCompositionTypes.Length)
        {
            // We must be missing one.
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.CompositionTypeNotFound, null);
        }

        var allPropertyTypeAliases = allCompositionTypes.SelectMany(x => x.CompositionPropertyTypes).Select(x => x.Alias).ToList();
        // Add all the aliases we're going to try to add as well.
        allPropertyTypeAliases.AddRange(model.Properties.Select(x => x.Alias));
        IEnumerable<string> allPropertyTypeAliasesDistinct = allPropertyTypeAliases.Distinct();
        if (allPropertyTypeAliases.Count != allPropertyTypeAliasesDistinct.Count())
        {
            // If our list shrank when doing distinct there was duplicates.
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.DuplicatePropertyTypeAlias, null);
        }

        // We only care about the keys used for composition.
        if (compositionKeys
            .Except(allowedCompositionKeys)
            .Any())
        {
            // We have a composition key that's not in the allowed composition keys
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidComposition, null);
        }

        // filter out properties and containers with no name/alias
        // TODO: Let's be predictable and fail instead of trying to guess intentions.
        model.Properties = model.Properties.Where(propertyType => propertyType.Alias.IsNullOrWhiteSpace() is false).ToArray();
        model.Containers = model.Containers.Where(container => container.Name.IsNullOrWhiteSpace() is false).ToArray();

        // figure out the content type parent; it is either
        // - the specified composition of type inheritance (the content type has a parent content type)
        // - the specified parent ID (the content type is placed in a container/folder)
        // - root if none of the above
        int? parentId;
        if (inheritedKeys.Any())
        {
            Attempt<int> parentContentTypeIdAttempt = _entityService.GetId(inheritedKeys.First(), UmbracoObjectTypes.DocumentType);
            if (parentContentTypeIdAttempt.Success is false)
            {
                return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidInheritance, null);
            }

            parentId = parentContentTypeIdAttempt.Result;
        }
        else if (model.ParentKey.HasValue)
        {
            Attempt<int> containerIdAttempt = _entityService.GetId(model.ParentKey.Value, UmbracoObjectTypes.DocumentTypeContainer);
            if (containerIdAttempt.Success is false)
            {
                return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.ParentNotFound, null);
            }

            parentId = containerIdAttempt.Result;
        }
        else
        {
            parentId = Constants.System.Root;
        }

        var contentType = new ContentType(_shortStringHelper, parentId.Value);

        // update basic content type settings
        // We want to allow the FE to specify a key
        if (model.Key is not null)
        {
            contentType.Key = model.Key.Value;
        }

        contentType.Alias = model.Alias;
        contentType.Description = model.Description;
        contentType.Icon = model.Icon;
        contentType.IsElement = model.IsElement;
        contentType.Name = model.Name;
        contentType.AllowedAsRoot = model.AllowedAsRoot;
        contentType.SetVariesBy(ContentVariation.Culture, model.VariesByCulture);
        contentType.SetVariesBy(ContentVariation.Segment, model.VariesBySegment);

        // update allowed content types
        var allowedContentTypesUnchanged = contentType.AllowedContentTypes?
            .OrderBy(contentTypeSort => contentTypeSort.SortOrder)
            .Select(contentTypeSort => contentTypeSort.Key)
            .SequenceEqual(model.AllowedContentTypes
                .OrderBy(contentTypeSort => contentTypeSort.SortOrder)
                .Select(contentTypeSort => contentTypeSort.Key)) ?? false;
        if (allowedContentTypesUnchanged is false)
        {
            // need the content type IDs here - yet, anyway - see FIXME in Umbraco.Cms.Core.Models.ContentTypeSort
            var allContentTypesByKey = allContentTypes.ToDictionary(c => c.Key);
            contentType.AllowedContentTypes = model
                .AllowedContentTypes
                .Select((contentTypeSort, index) => allContentTypesByKey.TryGetValue(contentTypeSort.Key, out IContentType? ct)
                    ? new ContentTypeSort(new Lazy<int>(() => ct.Id), contentTypeSort.Key, index, ct.Alias)
                    : null)
                .WhereNotNull()
                .ToArray();
        }

        // build a dictionary of parent container IDs and their names (we need it when mapping property groups)
        var parentContainerNamesById = model
            .Containers
            .Where(container => container.ParentKey is not null)
            .DistinctBy(container => container.ParentKey)
            .ToDictionary(
                container => container.ParentKey!.Value,
                container => model.Containers.First(c => c.Key == container.ParentKey).Name!);

        // FIXME: when refactoring for media and member types, this needs to be some kind of abstract implementation - media and member types do not support publishing
        const bool supportsPublishing = true;

        IPropertyType MapProperty(ContentTypePropertyTypeModel property, PropertyGroup? propertyGroup)
        {
            // get the selected data type
            // NOTE: this only works because we already ensured that the data type is present in the dataTypesByKey dictionary
            IDataType dataType = dataTypesByKey[property.DataTypeKey];

            // get the current property type (if it exists)
            IPropertyType propertyType = contentType.PropertyTypes.FirstOrDefault(pt => pt.Key == property.Key)
                                         ?? new PropertyType(_shortStringHelper, dataType);

            // We are demanding a property type key in the model, so we should probably ensure that it's the on that's actually used.
            propertyType.Key = property.Key;
            propertyType.Name = property.Name;
            propertyType.DataTypeId = dataType.Id;
            propertyType.DataTypeKey = dataType.Key;
            propertyType.Mandatory = property.Validation.Mandatory;
            propertyType.MandatoryMessage = property.Validation.MandatoryMessage;
            propertyType.ValidationRegExp = property.Validation.RegularExpression;
            propertyType.ValidationRegExpMessage = property.Validation.RegularExpressionMessage;
            propertyType.SetVariesBy(ContentVariation.Culture, property.VariesByCulture);
            propertyType.SetVariesBy(ContentVariation.Segment, property.VariesBySegment);
            propertyType.Alias = property.Alias;
            propertyType.Description = property.Description;
            propertyType.SortOrder = property.SortOrder;
            propertyType.LabelOnTop = property.Appearance.LabelOnTop;

            if (propertyGroup is not null)
            {
                propertyType.PropertyGroupId = new Lazy<int>(() => propertyGroup.Id, false);
            }

            return propertyType;
        }

        // update properties and groups
        PropertyGroup[] propertyGroups = model.Containers.Select(container =>
            {
                PropertyGroup propertyGroup = contentType.PropertyGroups.FirstOrDefault(group => group.Key == container.Key) ??
                                              new PropertyGroup(supportsPublishing) { Key = container.Key };
                // NOTE: eventually group.Type should be a string to make the client more flexible; for now we'll have to parse the string value back to its expected enum
                propertyGroup.Type = Enum.Parse<PropertyGroupType>(container.Type);
                propertyGroup.Name = container.Name;
                // this is not pretty, but this is how the data structure is at the moment; we just have to live with it for the time being.
                var alias = container.Name!;
                if (container.ParentKey is not null)
                {
                    alias = $"{parentContainerNamesById[container.ParentKey.Value]}/{alias}";
                }
                propertyGroup.Alias = alias;
                propertyGroup.SortOrder = container.SortOrder;

                IPropertyType[] properties = model
                    .Properties
                    .Where(property => property.ContainerKey == container.Key)
                    .Select(property => MapProperty(property, propertyGroup))
                    .ToArray();

                if (properties.Any() is false && parentContainerNamesById.ContainsKey(container.Key) is false)
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

        if (contentType.PropertyGroups.SequenceEqual(propertyGroups) is false)
        {
            contentType.PropertyGroups = new PropertyGroupCollection(propertyGroups);
        }

        // Handle orphaned properties
        IEnumerable<ContentTypePropertyTypeModel> orphanedPropertyTypeModels = model.Properties.Where (x => x.ContainerKey is null).ToArray();
        IPropertyType[] orphanedPropertyTypes = orphanedPropertyTypeModels.Select(property => MapProperty(property, null)).ToArray();
        if (contentType.NoGroupPropertyTypes.SequenceEqual(orphanedPropertyTypes) is false)
        {
            contentType.NoGroupPropertyTypes = new PropertyTypeCollection(supportsPublishing, orphanedPropertyTypes);
        }

        // Updates compositions
        // We don't actually have to worry about alias collision here because that's also checked in the service
        // We'll probably want to refactor this to be able to return a proper ContentTypeOperationStatus.
        // In the mapping step here we only really care about the most immediate ancestors.
        // We only really have to care about removing when updating
        Guid[] currentKeys = contentType.ContentTypeComposition.Select(x => x.Key).ToArray();
        Guid[] targetCompositionKeys = model.Compositions.Select(x => x.Key).ToArray();

        // We want to remove all of those that are in current, but not in targetCompositionKeys
        Guid[] remove = currentKeys.Except(targetCompositionKeys).ToArray();
        IEnumerable<Guid> add = targetCompositionKeys.Except(currentKeys).ToArray();

        foreach (Guid key in remove)
        {
            contentType.RemoveContentType(key);
        }

        // We have to look up the content types we want to add to composition, since we keep a full reference.
        if (add.Any())
        {
            IContentType[] contentTypesToAdd = allContentTypes.Where(c => add.Contains(c.Key)).ToArray();
            foreach (IContentType contentTypeToAdd in contentTypesToAdd)
            {
                contentType.AddContentType(contentTypeToAdd);
            }
        }

        // We need to handle the parent as well
        // We've already validated that there is at most one and that it exists if it there
        Guid? parentKey = inheritedKeys.Any() ? inheritedKeys.First() : null;
        if (parentKey.HasValue)
        {
            IContentType parentContentType = allContentTypes.First(c => c.Key == parentKey.Value);
            contentType.SetParent(parentContentType);
        }

        // update content type history clean-up
        contentType.HistoryCleanup ??= new HistoryCleanup();
        contentType.HistoryCleanup.PreventCleanup = model.Cleanup.PreventCleanup;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = model.Cleanup.KeepAllVersionsNewerThanDays;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = model.Cleanup.KeepLatestVersionPerDayForDays;

        // update allowed templates and assign default template
        ITemplate[] allowedTemplates = model.AllowedTemplateKeys
            .Select(async templateId => await _templateService.GetAsync(templateId))
            .Select(t => t.Result)
            .WhereNotNull()
            .ToArray();
        contentType.AllowedTemplates = allowedTemplates;
        // NOTE: incidentally this also covers removing the default template; when requestModel.DefaultTemplateId is null,
        //       contentType.SetDefaultTemplate() will be called with a null value, which will reset the default template.
        contentType.SetDefaultTemplate(allowedTemplates.FirstOrDefault(t => t.Key == model.DefaultTemplateKey));


        // save content type
        // FIXME: create and use an async get method here.
        _contentTypeService.Save(contentType);

        return Attempt.SucceedWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }
}

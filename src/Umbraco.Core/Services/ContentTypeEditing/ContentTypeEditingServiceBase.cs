﻿using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public abstract class ContentTypeEditingServiceBase<TContentType, TContentTypeService, TContentTypeCreateModel, TPropertyTypeModel, TPropertyTypeContainer>
    where TContentType : class, IContentTypeComposition
    where TContentTypeService : IContentTypeBaseService<TContentType>
    where TContentTypeCreateModel : ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer>, IContentTypeCreate
    where TPropertyTypeModel : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly TContentTypeService _concreteContentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IEntityService _entityService;
    private readonly IShortStringHelper _shortStringHelper;

    protected ContentTypeEditingServiceBase(
        IContentTypeService contentTypeService,
        TContentTypeService concreteContentTypeService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper)
    {
        _contentTypeService = contentTypeService;
        _concreteContentTypeService = concreteContentTypeService;
        _dataTypeService = dataTypeService;
        _entityService = entityService;
        _shortStringHelper = shortStringHelper;
    }

    protected abstract Guid[] GetAvailableCompositionKeys(IContentTypeComposition? source, IContentTypeComposition[] allContentTypes, bool isElement);

    protected abstract TContentType CreateContentType(IShortStringHelper shortStringHelper, int parentId);

    protected abstract bool SupportsPublishing { get; }

    protected abstract UmbracoObjectTypes ContentObjectType { get; }

    protected abstract UmbracoObjectTypes ContainerObjectType { get; }

    protected async Task<Attempt<TContentType?, ContentTypeOperationStatus>> HandleCreateAsync(TContentTypeCreateModel model)
    {
        // validate that this is a new content type alias
        if (ContentTypeAliasIsInUse(model.Alias))
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.DuplicateAlias, null);
        }

        // get all existing content types as content type compositions
        // NOTE: using Cast here is OK, because we implicitly enforce the constraint TContentType : IContentTypeComposition
        IContentTypeComposition[] allContentTypeCompositions = _concreteContentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();

        // now validate the model
        ContentTypeOperationStatus operationStatus = await ValidateAsync(model, model.ParentKey, allContentTypeCompositions);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(operationStatus, null);
        }

        var parentId = GetParentId(model, model.ParentKey) ?? throw new ArgumentException("Parent ID could not be found", nameof(model));
        TContentType contentType = CreateContentType(_shortStringHelper, parentId);

        // update basic content type settings
        // We want to allow the FE to specify a key
        if (model.Key is not null)
        {
            contentType.Key = model.Key.Value;
        }

        contentType = await UpdateAsync(contentType, model, allContentTypeCompositions);
        return Attempt.SucceedWithStatus<TContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }

    #region Model validation

    private async Task<ContentTypeOperationStatus> ValidateAsync(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, Guid? parentKey, IContentTypeComposition[] allContentTypeCompositions)
    {
        // validate all model aliases (content type alias, property type aliases)
        ContentTypeOperationStatus operationStatus = ValidateModelAliases(model);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return operationStatus;
        }

        // validate property data types exists.
        operationStatus = await ValidateDataTypesAsync(model);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return operationStatus;
        }

        // validate inheritance and find the correct content type parent ID (can be both a parent content type and a container)
        operationStatus = ValidateParent(model, parentKey);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return operationStatus;
        }

        // TODO: handle update here (contentType is not null)
        // verify that all compositions are valid)
        operationStatus = ValidateCompositions(null, model, allContentTypeCompositions);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return operationStatus;
        }

        operationStatus = ValidateProperties(model, allContentTypeCompositions);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return operationStatus;
        }

        return ContentTypeOperationStatus.Success;
    }

    private ContentTypeOperationStatus ValidateModelAliases(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        // Validate model alias is not reserved.
        if (IsReservedContentTypeAlias(model.Alias))
        {
            return ContentTypeOperationStatus.InvalidAlias;
        }

        // Validate properties for reserved aliases.
        if (ContainsReservedPropertyTypeAlias(model))
        {
            return ContentTypeOperationStatus.InvalidPropertyTypeAlias;
        }

        // properties must have aliases
        if (model.Properties.Any(p => p.Alias.IsNullOrWhiteSpace()))
        {
            return ContentTypeOperationStatus.InvalidPropertyTypeAlias;
        }

        // containers must names
        if (model.Containers.Any(p => p.Name.IsNullOrWhiteSpace()))
        {
            return ContentTypeOperationStatus.InvalidContainerName;
        }

        return ContentTypeOperationStatus.Success;
    }

    private async Task<ContentTypeOperationStatus> ValidateDataTypesAsync(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        Guid[] dataTypeKeys = GetDataTypeKeys(model);
        IDataType[] dataTypes = await GetDataTypesAsync(model);

        if (dataTypeKeys.Length != dataTypes.Length)
        {
            return ContentTypeOperationStatus.DataTypeNotFound;
        }

        return ContentTypeOperationStatus.Success;
    }

    private ContentTypeOperationStatus ValidateParent(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, Guid? parentKey)
    {
        Guid[] inheritedKeys = KeysForCompositionTypes(model, CompositionType.Inheritance);
        Guid[] compositionKeys = KeysForCompositionTypes(model, CompositionType.Composition);

        // Only one composition can be of type inheritance, and composed items cannot also be inherited.
        if (inheritedKeys.Length > 1 || compositionKeys.Intersect(inheritedKeys).Any())
        {
            return ContentTypeOperationStatus.InvalidInheritance;
        }

        // a content type cannot be created/saved in an entity container (a folder) if has an inheritance type composition
        if (inheritedKeys.Any() && parentKey.HasValue)
        {
            return ContentTypeOperationStatus.InvalidParent;
        }

        var parentId = GetParentId(model, parentKey);
        if (parentId.HasValue)
        {
            return ContentTypeOperationStatus.Success;
        }

        // no parent ID => must be either an invalid inheritance (if attempted) or an invalid container
        return inheritedKeys.Any()
            ? ContentTypeOperationStatus.InvalidInheritance
            : ContentTypeOperationStatus.InvalidParent;
    }

    private ContentTypeOperationStatus ValidateCompositions(TContentType? contentType, ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, IContentTypeComposition[] allContentTypeCompositions)
    {
        // get the content type keys we want to use for compositions
        Guid[] compositionKeys = KeysForCompositionTypes(model, CompositionType.Composition);

        // verify that all compositions keys are allowed
        Guid[] allowedCompositionKeys = GetAvailableCompositionKeys(contentType, allContentTypeCompositions, model.IsElement);
        if (allowedCompositionKeys.ContainsAll(compositionKeys) is false)
        {
            return ContentTypeOperationStatus.InvalidComposition;
        }

        return ContentTypeOperationStatus.Success;
    }

    private ContentTypeOperationStatus ValidateProperties(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, IContentTypeComposition[] allContentTypeCompositions)
    {
        // grab all content types used for composition and/or inheritance
        Guid[] allCompositionKeys = KeysForCompositionTypes(model, CompositionType.Composition, CompositionType.Inheritance);
        IContentTypeComposition[] allCompositionTypes = allContentTypeCompositions.Where(c => allCompositionKeys.Contains(c.Key)).ToArray();

        // get the aliases of all properties across these content types
        var allPropertyTypeAliases = allCompositionTypes.SelectMany(x => x.CompositionPropertyTypes).Select(x => x.Alias).ToList();

        // add all the aliases we're going to try to add as well
        allPropertyTypeAliases.AddRange(model.Properties.Select(x => x.Alias));
        if (allPropertyTypeAliases.HasDuplicates(true))
        {
            return ContentTypeOperationStatus.DuplicatePropertyTypeAlias;
        }

        return ContentTypeOperationStatus.Success;
    }

    // This this method gets aliases across documents, members, and media, so it covers it all
    private bool ContentTypeAliasIsInUse(string alias) => _contentTypeService.GetAllContentTypeAliases().Contains(alias);

    private bool IsReservedContentTypeAlias(string alias)
    {
        var reservedAliases = new[] { "system" };
        return reservedAliases.InvariantContains(alias);
    }

    private bool ContainsReservedPropertyTypeAlias(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        // Because of models builder you cannot have an alias that already exists in IPublishedContent, for instance Path.
        // Since MyModel.Path would conflict with IPublishedContent.Path.
        var reservedPropertyTypeNames = typeof(IPublishedContent).GetProperties().Select(x => x.Name)
            .Union(typeof(IPublishedContent).GetMethods().Select(x => x.Name))
            .ToArray();

        return model.Properties.Any(propertyType => propertyType.Alias.Equals(model.Alias, StringComparison.OrdinalIgnoreCase)
                                                   || reservedPropertyTypeNames.InvariantContains(propertyType.Alias));
    }

    #endregion

    #region Model update

    private async Task<TContentType> UpdateAsync(
        TContentType contentType,
        ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model,
        IContentTypeComposition[] allContentTypeCompositions)
    {
        contentType.Alias = model.Alias;
        contentType.Description = model.Description;
        contentType.Icon = model.Icon;
        contentType.Name = model.Name;
        contentType.AllowedAsRoot = model.AllowedAsRoot;
        contentType.SetVariesBy(ContentVariation.Culture, model.VariesByCulture);
        contentType.SetVariesBy(ContentVariation.Segment, model.VariesBySegment);

        // update the allowed content types
        UpdateAllowedContentTypes(contentType, model, allContentTypeCompositions);

        // update/map all properties
        await UpdatePropertiesAsync(contentType, model);

        // update all compositions
        UpdateCompositions(contentType, model, allContentTypeCompositions);

        // ensure parent content type assignment (inheritance) if any
        UpdateParentContentType(contentType, model, allContentTypeCompositions);

        return contentType;
    }

    private void UpdateAllowedContentTypes(
        TContentType contentType,
        ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model,
        IContentTypeComposition[] allContentTypeCompositions)
    {
        var allowedContentTypesUnchanged = contentType.AllowedContentTypes?
            .OrderBy(contentTypeSort => contentTypeSort.SortOrder)
            .Select(contentTypeSort => contentTypeSort.Key)
            .SequenceEqual(model.AllowedContentTypes
                .OrderBy(contentTypeSort => contentTypeSort.SortOrder)
                .Select(contentTypeSort => contentTypeSort.Key)) ?? false;

        if (allowedContentTypesUnchanged)
        {
            return;
        }

        // need the content type IDs here - yet, anyway - see FIXME in Umbraco.Cms.Core.Models.ContentTypeSort
        var allContentTypesByKey = allContentTypeCompositions.ToDictionary(c => c.Key);
        contentType.AllowedContentTypes = model
            .AllowedContentTypes
            .Select((contentTypeSort, index) => allContentTypesByKey.TryGetValue(contentTypeSort.Key, out IContentTypeComposition? ct)
                ? new ContentTypeSort(new Lazy<int>(() => ct.Id), contentTypeSort.Key, index, ct.Alias)
                : null)
            .WhereNotNull()
            .ToArray();
    }

    private async Task UpdatePropertiesAsync(
        TContentType contentType,
        ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        // build a dictionary of all data types within the model by their keys (we need it when mapping properties)
        var dataTypesByKey = (await GetDataTypesAsync(model)).ToDictionary(d => d.Key);

        // build a dictionary of parent container IDs and their names (we need it when mapping property groups)
        var parentContainerNamesById = model
            .Containers
            .Where(container => container.ParentKey is not null)
            .DistinctBy(container => container.ParentKey)
            .ToDictionary(
                container => container.ParentKey!.Value,
                container => model.Containers.First(c => c.Key == container.ParentKey).Name!);

        // handle properties in groups
        PropertyGroup[] propertyGroups = model.Containers.Select(container =>
            {
                PropertyGroup propertyGroup = contentType.PropertyGroups.FirstOrDefault(group => group.Key == container.Key) ??
                                              new PropertyGroup(SupportsPublishing) { Key = container.Key };
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
                    .Select(property => MapProperty(contentType, property, propertyGroup, dataTypesByKey))
                    .ToArray();

                if (properties.Any() is false && parentContainerNamesById.ContainsKey(container.Key) is false)
                {
                    // FIXME: if at all possible, retain empty containers (bad DX to remove stuff that's been attempted saved)
                    return null;
                }

                if (propertyGroup.PropertyTypes == null || propertyGroup.PropertyTypes.SequenceEqual(properties) is false)
                {
                    propertyGroup.PropertyTypes = new PropertyTypeCollection(SupportsPublishing, properties);
                }

                return propertyGroup;
            })
            .WhereNotNull()
            .ToArray();

        if (contentType.PropertyGroups.SequenceEqual(propertyGroups) is false)
        {
            contentType.PropertyGroups = new PropertyGroupCollection(propertyGroups);
        }

        // handle orphaned properties
        IEnumerable<TPropertyTypeModel> orphanedPropertyTypeModels = model.Properties.Where (x => x.ContainerKey is null).ToArray();
        IPropertyType[] orphanedPropertyTypes = orphanedPropertyTypeModels.Select(property => MapProperty(contentType, property, null, dataTypesByKey)).ToArray();
        if (contentType.NoGroupPropertyTypes.SequenceEqual(orphanedPropertyTypes) is false)
        {
            contentType.NoGroupPropertyTypes = new PropertyTypeCollection(SupportsPublishing, orphanedPropertyTypes);
        }
    }

    private IPropertyType MapProperty(
        TContentType contentType,
        TPropertyTypeModel property,
        PropertyGroup? propertyGroup,
        IDictionary<Guid, IDataType> dataTypesByKey)
    {
        // get the selected data type
        // NOTE: this only works because we already ensured that the data type is present in the dataTypesByKey dictionary
        if (dataTypesByKey.TryGetValue(property.DataTypeKey, out IDataType? dataType) is false)
        {
            throw new ArgumentException("One or more data types could not be found", nameof(dataTypesByKey));
        }

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

    private void UpdateCompositions(
        TContentType contentType,
        ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model,
        IContentTypeComposition[] allContentTypeCompositions)
    {
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
            IContentTypeComposition[] contentTypesToAdd = allContentTypeCompositions.Where(c => add.Contains(c.Key)).ToArray();
            foreach (IContentTypeComposition contentTypeToAdd in contentTypesToAdd)
            {
                contentType.AddContentType(contentTypeToAdd);
            }
        }
    }

    private void UpdateParentContentType(
        TContentType contentType,
        ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model,
        IContentTypeComposition[] allContentTypeCompositions)
    {
        // at this point, we should have already validated that there is at most one and that it exists if it there
        Guid parentContentTypeKey = KeysForCompositionTypes(model, CompositionType.Inheritance).FirstOrDefault();
        if (parentContentTypeKey != Guid.Empty)
        {
            IContentTypeComposition parentContentType = allContentTypeCompositions.FirstOrDefault(c => c.Key == parentContentTypeKey)
                                                        ?? throw new ArgumentException("Parent content type could not be found", nameof(model));
            contentType.SetParent(parentContentType);
        }
    }

    #endregion

    #region Shared between model validation and model update

    private Guid[] GetDataTypeKeys(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
        => model.Properties.Select(property => property.DataTypeKey).Distinct().ToArray();

    private async Task<IDataType[]> GetDataTypesAsync(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
        => (await _dataTypeService.GetAllAsync(GetDataTypeKeys(model))).ToArray();

    private int? GetParentId(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, Guid? parentKey)
    {
        Guid[] inheritedKeys = KeysForCompositionTypes(model, CompositionType.Inheritance);

        // figure out the content type parent; it is either
        // - the specified composition of type inheritance (the content type has a parent content type)
        // - the specified parent ID (the content type is placed in a container/folder)
        // - root if none of the above
        if (inheritedKeys.Any())
        {
            Attempt<int> parentContentTypeIdAttempt = _entityService.GetId(inheritedKeys.First(), ContentObjectType);
            return parentContentTypeIdAttempt.Success ? parentContentTypeIdAttempt.Result : null;
        }

        if (parentKey.HasValue)
        {
            Attempt<int> containerIdAttempt = _entityService.GetId(parentKey.Value, ContainerObjectType);
            return containerIdAttempt.Success ? containerIdAttempt.Result : null;
        }

        return Constants.System.Root;
    }

    private Guid[] KeysForCompositionTypes(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, params CompositionType[] compositionTypes)
        => model.Compositions
            .Where(c => compositionTypes.Contains(c.CompositionType))
            .Select(c => c.Key)
            .ToArray();

    #endregion
}

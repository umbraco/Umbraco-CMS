using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

internal abstract class ContentTypeEditingServiceBase<TContentType, TContentTypeService, TPropertyTypeModel, TPropertyTypeContainer>
    where TContentType : class, IContentTypeComposition
    where TContentTypeService : IContentTypeBaseService<TContentType>
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

    protected abstract TContentType CreateContentType(IShortStringHelper shortStringHelper, int parentId);

    protected abstract bool SupportsPublishing { get; }

    protected abstract UmbracoObjectTypes ContentTypeObjectType { get; }

    protected abstract UmbracoObjectTypes ContainerObjectType { get; }

    protected async Task<IEnumerable<ContentTypeAvailableCompositionsResult>> FindAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases,
        bool isElement = false)
    {
        TContentType? contentType = key.HasValue ? await _concreteContentTypeService.GetAsync(key.Value) : null;
        IContentTypeComposition[] allContentTypes = _concreteContentTypeService.GetAll().ToArray();

        var currentCompositionAliases = currentCompositeKeys.Any()
            ? allContentTypes.Where(ct => currentCompositeKeys.Contains(ct.Key)).Select(ct => ct.Alias).ToArray()
            : Array.Empty<string>();

        ContentTypeAvailableCompositionsResults availableCompositions = _contentTypeService.GetAvailableCompositeContentTypes(
            contentType,
            allContentTypes,
            currentCompositionAliases,
            currentPropertyAliases.ToArray(),
            isElement);

        return availableCompositions.Results;
    }

    protected async Task<Attempt<TContentType?, ContentTypeOperationStatus>> ValidateAndMapForCreationAsync(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, Guid? key, Guid? containerKey)
    {
        SanitizeModelAliases(model);

        // validate that this is a new content type alias
        if (ContentTypeAliasIsInUse(model.Alias))
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.DuplicateAlias, null);
        }

        // get all existing content type compositions
        IContentTypeComposition[] allContentTypeCompositions = GetAllContentTypeCompositions();

        // validate inheritance or parent container - a content type can be created either under another content type (inheritance) or inside a container (folder)
        ContentTypeOperationStatus operationStatus = ValidateInheritanceAndParent(model, containerKey);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(operationStatus, null);
        }

        // validate the rest of the model
        operationStatus = await ValidateAsync(model, null, allContentTypeCompositions);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(operationStatus, null);
        }

        // perform additional, content type specific validation
        operationStatus = await AdditionalCreateValidationAsync(model);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(operationStatus, null);
        }

        // get the ID of the parent to create the content type under (we already validated that it exists)
        var parentId = GetParentId(model, containerKey) ?? throw new ArgumentException("Parent ID could not be found", nameof(model));
        TContentType contentType = CreateContentType(_shortStringHelper, parentId);

        // if the key is specified explicitly, set it (create only)
        if (key is not null)
        {
            contentType.Key = key.Value;
        }

        // map the model to the content type
        contentType = await UpdateAsync(contentType, model, allContentTypeCompositions);
        return Attempt.SucceedWithStatus<TContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }

    protected async Task<Attempt<TContentType?, ContentTypeOperationStatus>> ValidateAndMapForUpdateAsync(TContentType contentType, ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        SanitizeModelAliases(model);

        if (ContentTypeAliasCanBeUsedFor(model.Alias, contentType.Key) is false)
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.InvalidAlias, null);
        }

        // get all existing content type compositions
        IContentTypeComposition[] allContentTypeCompositions = GetAllContentTypeCompositions();

        // validate that inheritance or parent relationship hasn't changed
        ContentTypeOperationStatus operationStatus = ValidateInheritanceAndParent(contentType, model);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(operationStatus, null);
        }

        // validate the rest of the model
        operationStatus = await ValidateAsync(model, contentType, allContentTypeCompositions);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContentType?, ContentTypeOperationStatus>(operationStatus, null);
        }

        // map the model to the content type
        contentType = await UpdateAsync(contentType, model, allContentTypeCompositions);
        return Attempt.SucceedWithStatus<TContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }

    protected virtual async Task<ContentTypeOperationStatus> AdditionalCreateValidationAsync(
        ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
        => await Task.FromResult(ContentTypeOperationStatus.Success);

    #region Sanitization

    private void SanitizeModelAliases(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        model.Alias = model.Alias.ToSafeAlias(_shortStringHelper);
        foreach (TPropertyTypeModel property in model.Properties)
        {
            property.Alias = property.Alias.ToSafeAlias(_shortStringHelper);
        }
    }

    #endregion

    #region Model validation

    private async Task<ContentTypeOperationStatus> ValidateAsync(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, TContentType? contentType, IContentTypeComposition[] allContentTypeCompositions)
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

        // verify that all compositions are valid
        operationStatus = ValidateCompositions(contentType, model, allContentTypeCompositions);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return operationStatus;
        }

        // verify that all properties are valid
        operationStatus = ValidateProperties(model, allContentTypeCompositions);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return operationStatus;
        }

        // verify that all property/container relationships (groups/tabs) are valid
        operationStatus = ValidateContainers(model, allContentTypeCompositions);
        if (operationStatus is not ContentTypeOperationStatus.Success)
        {
            return operationStatus;
        }

        return ContentTypeOperationStatus.Success;
    }

    private ContentTypeOperationStatus ValidateModelAliases(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        // Validate model alias is not reserved.
        if (IsReservedContentTypeAlias(model.Alias) || IsUnsafeAlias(model.Alias))
        {
            return ContentTypeOperationStatus.InvalidAlias;
        }

        // Validate content type alias is not in use.
        if (model.Properties.Any(propertyType => propertyType.Alias.Equals(model.Alias, StringComparison.OrdinalIgnoreCase)))
        {
            return ContentTypeOperationStatus.PropertyTypeAliasCannotEqualContentTypeAlias;
        }

        // Validate properties for reserved aliases.
        if (ContainsReservedPropertyTypeAlias(model))
        {
            return ContentTypeOperationStatus.InvalidPropertyTypeAlias;
        }

        // properties must have aliases
        if (model.Properties.Any(p => IsUnsafeAlias(p.Alias)))
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

    private ContentTypeOperationStatus ValidateInheritanceAndParent(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, Guid? containerKey)
    {
        Guid[] inheritedKeys = KeysForCompositionTypes(model, CompositionType.Inheritance);
        Guid[] compositionKeys = KeysForCompositionTypes(model, CompositionType.Composition);

        // Only one composition can be of type inheritance, and composed items cannot also be inherited.
        if (inheritedKeys.Length > 1 || compositionKeys.Intersect(inheritedKeys).Any())
        {
            return ContentTypeOperationStatus.InvalidInheritance;
        }

        // a content type cannot be created/saved in an entity container (a folder) if has an inheritance type composition
        if (inheritedKeys.Any() && containerKey.HasValue)
        {
            return ContentTypeOperationStatus.InvalidParent;
        }

        var parentId = GetParentId(model, containerKey);
        if (parentId.HasValue)
        {
            return ContentTypeOperationStatus.Success;
        }

        // no parent ID => must be either an invalid inheritance (if attempted) or an invalid container
        return inheritedKeys.Any()
            ? ContentTypeOperationStatus.InvalidInheritance
            : ContentTypeOperationStatus.InvalidParent;
    }

    private ContentTypeOperationStatus ValidateInheritanceAndParent(TContentType contentType, ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        Guid[] inheritedKeys = KeysForCompositionTypes(model, CompositionType.Inheritance);
        if (inheritedKeys.Length > 1)
        {
            return ContentTypeOperationStatus.InvalidInheritance;
        }

        if (contentType.ParentId == Constants.System.Root)
        {
            // the content type does not inherit from another content type, nor does it reside in a container
            return inheritedKeys.Any()
                ? ContentTypeOperationStatus.InvalidInheritance
                : ContentTypeOperationStatus.Success;
        }

        Attempt<Guid> parentContentTypeKeyAttempt = _entityService.GetKey(contentType.ParentId, ContentTypeObjectType);
        if (parentContentTypeKeyAttempt.Success)
        {
            // the content type inherits from another content type - the model must specify that content type as inheritance
            return inheritedKeys.Any() is false || inheritedKeys.First() != parentContentTypeKeyAttempt.Result
                ? ContentTypeOperationStatus.InvalidInheritance
                : ContentTypeOperationStatus.Success;
        }

        Attempt<Guid> parentContainerKeyAttempt = _entityService.GetKey(contentType.ParentId, ContainerObjectType);
        if (parentContainerKeyAttempt.Success)
        {
            // the content resides within a container (folder) - the model must not specify any inheritance
            return inheritedKeys.Any()
                ? ContentTypeOperationStatus.InvalidInheritance
                : ContentTypeOperationStatus.Success;
        }

        // something went terribly wrong here; the existing content type parent ID does not match the root, another
        // content type or a container. this should not be possible.
        throw new ArgumentException("The content type parent ID does not match another content type, nor a container", nameof(contentType));
    }

    private ContentTypeOperationStatus ValidateCompositions(TContentType? contentType, ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, IContentTypeComposition[] allContentTypeCompositions)
    {
        // get the content type keys we want to use for compositions
        Guid[] compositionKeys = KeysForCompositionTypes(model, CompositionType.Composition);

        // verify that all compositions keys are allowed
        Guid[] allowedCompositionKeys = _contentTypeService.GetAvailableCompositeContentTypes(contentType, allContentTypeCompositions, isElement: model.IsElement)
            .Results
            .Where(x => x.Allowed)
            .Select(x => x.Composition.Key)
            .ToArray();

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
        if (allPropertyTypeAliases.Select(a => a.ToLowerInvariant()).HasDuplicates(true))
        {
            return ContentTypeOperationStatus.DuplicatePropertyTypeAlias;
        }

        return ContentTypeOperationStatus.Success;
    }

    private ContentTypeOperationStatus ValidateContainers(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, IContentTypeComposition[] allContentTypeCompositions)
    {
        if (model.Containers.Any(container => Enum.TryParse<PropertyGroupType>(container.Type, out _) is false))
        {
            return ContentTypeOperationStatus.InvalidContainerType;
        }

        // all property container keys must be present in the model
        Guid[] modelContainerKeys = model.Containers.Select(c => c.Key).ToArray();
        if (model.Properties.Any(p => p.ContainerKey is not null && modelContainerKeys.Contains(p.ContainerKey.Value) is false))
        {
            return ContentTypeOperationStatus.MissingContainer;
        }

        // duplicate container keys are not allowed
        if (modelContainerKeys.Distinct().Count() != modelContainerKeys.Length)
        {
            return ContentTypeOperationStatus.DuplicateContainer;
        }

        // all container parent keys must also be present in the model
        if (model.Containers.Any(c => c.ParentKey.HasValue && modelContainerKeys.Contains(c.ParentKey.Value) is false))
        {
            return ContentTypeOperationStatus.MissingContainer;
        }

        // make sure no container keys in the model originate from compositions
        Guid[] allCompositionKeys = KeysForCompositionTypes(model, CompositionType.Composition, CompositionType.Inheritance);
        Guid[] compositionContainerKeys = allContentTypeCompositions
            .Where(c => allCompositionKeys.Contains(c.Key))
            .SelectMany(c => c.CompositionPropertyGroups.Select(g => g.Key))
            .Distinct()
            .ToArray();
        if (model.Containers.Any(c => compositionContainerKeys.Contains(c.Key)))
        {
            return ContentTypeOperationStatus.DuplicateContainer;
        }

        return ContentTypeOperationStatus.Success;
    }

    // This this method gets aliases across documents, members, and media, so it covers it all
    private bool ContentTypeAliasIsInUse(string alias) => _contentTypeService.GetAllContentTypeAliases().Contains(alias);

    private bool ContentTypeAliasCanBeUsedFor(string alias, Guid key)
    {
        IContentType? existingContentType = _contentTypeService.Get(alias);
        if (existingContentType is null || existingContentType.Key == key)
        {
            return true;
        }

        return ContentTypeAliasIsInUse(alias) is false;
    }

    private bool IsReservedContentTypeAlias(string alias)
    {
        var reservedAliases = new[] { "system" };
        return reservedAliases.InvariantContains(alias);
    }

    protected abstract ISet<string> GetReservedFieldNames();

    private bool ContainsReservedPropertyTypeAlias(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        // Because of models builder you cannot have an alias that already exists in IPublishedContent, for instance Path.
        // Since MyModel.Path would conflict with IPublishedContent.Path.
        ISet<string> reservedPropertyTypeNames = GetReservedFieldNames();

        return model.Properties.Any(propertyType => reservedPropertyTypeNames.InvariantContains(propertyType.Alias));
    }

    private bool IsUnsafeAlias(string alias) => alias.IsNullOrWhiteSpace()
                                                || alias.Length != alias.ToSafeAlias(_shortStringHelper).Length;

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
        contentType.IsElement = model.IsElement;
        contentType.ListView = model.ListView;
        contentType.SetVariesBy(ContentVariation.Culture, model.VariesByCulture);
        contentType.SetVariesBy(ContentVariation.Segment, model.VariesBySegment);

        // update the allowed content types
        UpdateAllowedContentTypes(contentType, model, allContentTypeCompositions);

        // update all compositions
        UpdateCompositions(contentType, model, allContentTypeCompositions);

        // ensure parent content type assignment (inheritance) if any
        UpdateParentContentType(contentType, model, allContentTypeCompositions);

        // update/map all properties
        await UpdatePropertiesAsync(contentType, model);

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

        var allContentTypesByKey = allContentTypeCompositions.ToDictionary(c => c.Key);
        contentType.AllowedContentTypes = model
            .AllowedContentTypes
            .OrderBy(contentTypeSort => contentTypeSort.SortOrder)
            .Select((contentTypeSort, index) => allContentTypesByKey.TryGetValue(contentTypeSort.Key, out IContentTypeComposition? ct)
                ? new ContentTypeSort(contentTypeSort.Key, index, ct.Alias)
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
                // NOTE: this look-up appears to be a little dangerous, but at this point we should have validated
                //       the containers and their parent relationships in the model, so it's ok
                container => model.Containers.First(c => c.Key == container.ParentKey).Name);

        // handle properties in groups
        PropertyGroup[] propertyGroups = model.Containers.Select(container =>
            {
                PropertyGroup propertyGroup = contentType.PropertyGroups.FirstOrDefault(group => group.Key == container.Key) ??
                                              new PropertyGroup(SupportsPublishing) { Key = container.Key };
                // NOTE: eventually group.Type should be a string to make the client more flexible; for now we'll have to parse the string value back to its expected enum
                propertyGroup.Type = Enum.Parse<PropertyGroupType>(container.Type);
                propertyGroup.Name = container.Name;
                // this is not pretty, but this is how the data structure is at the moment; we just have to live with it for the time being.
                var alias = PropertyGroupAlias(container.Name);
                if (container.ParentKey is not null)
                {
                    alias = $"{PropertyGroupAlias(parentContainerNamesById[container.ParentKey.Value])}/{alias}";
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

    private string PropertyGroupAlias(string? containerName)
    {
        if (containerName.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Container name cannot be empty", nameof(containerName));
        }

        var parts = containerName.Split(Constants.CharArrays.Space);
        return $"{parts.First().ToFirstLowerInvariant()}{string.Join(string.Empty, parts.Skip(1).Select(part => part.ToFirstUpperInvariant()))}";
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
    {
        Guid[] dataTypeKeys = GetDataTypeKeys(model);
        return dataTypeKeys.Any()
            ? (await _dataTypeService.GetAllAsync(GetDataTypeKeys(model))).ToArray()
            : Array.Empty<IDataType>();
    }

    private int? GetParentId(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, Guid? containerKey)
    {
        Guid[] inheritedKeys = KeysForCompositionTypes(model, CompositionType.Inheritance);

        // figure out the content type parent; it is either
        // - the specified composition of type inheritance (the content type has a parent content type)
        // - the specified parent ID (the content type is placed in a container/folder)
        // - root if none of the above
        if (inheritedKeys.Any())
        {
            Attempt<int> parentContentTypeIdAttempt = _entityService.GetId(inheritedKeys.First(), ContentTypeObjectType);
            return parentContentTypeIdAttempt.Success ? parentContentTypeIdAttempt.Result : null;
        }

        if (containerKey.HasValue)
        {
            Attempt<int> containerIdAttempt = _entityService.GetId(containerKey.Value, ContainerObjectType);
            return containerIdAttempt.Success ? containerIdAttempt.Result : null;
        }

        return Constants.System.Root;
    }

    private Guid[] KeysForCompositionTypes(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, params CompositionType[] compositionTypes)
        => model.Compositions
            .Where(c => compositionTypes.Contains(c.CompositionType))
            .Select(c => c.Key)
            .ToArray();

    private IContentTypeComposition[] GetAllContentTypeCompositions()
        // NOTE: using Cast here is OK, because we implicitly enforce the constraint TContentType : IContentTypeComposition
        => _concreteContentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();

    #endregion
}

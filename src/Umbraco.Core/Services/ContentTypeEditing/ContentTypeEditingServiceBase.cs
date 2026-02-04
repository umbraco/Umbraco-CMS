using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Abstract base class providing common functionality for content type editing services.
/// </summary>
/// <typeparam name="TContentType">The type of content type being edited (e.g., <see cref="IContentType"/>, <see cref="IMediaType"/>, <see cref="IMemberType"/>).</typeparam>
/// <typeparam name="TContentTypeService">The service type for managing the content type.</typeparam>
/// <typeparam name="TPropertyTypeModel">The model type for property type definitions.</typeparam>
/// <typeparam name="TPropertyTypeContainer">The model type for property type containers (groups/tabs).</typeparam>
/// <remarks>
///     This base class provides shared validation, mapping, and update logic for all content type
///     editing services including document types, media types, and member types.
/// </remarks>
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeEditingServiceBase{TContentType, TContentTypeService, TPropertyTypeModel, TPropertyTypeContainer}"/> class.
    /// </summary>
    /// <param name="contentTypeService">The content type service for cross-type operations.</param>
    /// <param name="concreteContentTypeService">The specific content type service for the type being edited.</param>
    /// <param name="dataTypeService">The data type service for validating property data types.</param>
    /// <param name="entityService">The entity service for resolving entity relationships.</param>
    /// <param name="shortStringHelper">The helper for generating safe aliases.</param>
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

    /// <summary>
    ///     Creates a new content type instance.
    /// </summary>
    /// <param name="shortStringHelper">The helper for generating safe aliases.</param>
    /// <param name="parentId">The ID of the parent content type or container.</param>
    /// <returns>A new instance of the content type.</returns>
    protected abstract TContentType CreateContentType(IShortStringHelper shortStringHelper, int parentId);

    /// <summary>
    ///     Gets a value indicating whether this content type supports publishing workflow.
    /// </summary>
    /// <value><c>true</c> for document types; <c>false</c> for media and member types.</value>
    protected abstract bool SupportsPublishing { get; }

    /// <summary>
    ///     Gets the Umbraco object type for this content type.
    /// </summary>
    protected abstract UmbracoObjectTypes ContentTypeObjectType { get; }

    /// <summary>
    ///     Gets the Umbraco object type for containers (folders) of this content type.
    /// </summary>
    protected abstract UmbracoObjectTypes ContainerObjectType { get; }

    /// <summary>
    ///     Finds available compositions for a content type.
    /// </summary>
    /// <param name="key">The unique identifier of the content type, or <c>null</c> for a new content type.</param>
    /// <param name="currentCompositeKeys">The keys of currently selected compositions.</param>
    /// <param name="currentPropertyAliases">The aliases of properties currently defined on the content type.</param>
    /// <param name="isElement">Whether the content type is configured as an element type.</param>
    /// <returns>A collection of available composition results.</returns>
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
            : [];

        ContentTypeAvailableCompositionsResults availableCompositions = _contentTypeService.GetAvailableCompositeContentTypes(
            contentType,
            allContentTypes,
            currentCompositionAliases,
            currentPropertyAliases.ToArray(),
            isElement);

        return availableCompositions.Results;
    }

    /// <summary>
    ///     Validates and maps a model for creating a new content type.
    /// </summary>
    /// <param name="model">The creation model to validate and map.</param>
    /// <param name="key">The optional explicit key for the new content type.</param>
    /// <param name="containerKey">The optional key of the container (folder) to create the content type in.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}"/> containing the mapped content type on success,
    ///     or a <see cref="ContentTypeOperationStatus"/> indicating the validation failure reason.
    /// </returns>
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

    /// <summary>
    ///     Validates and maps a model for updating an existing content type.
    /// </summary>
    /// <param name="contentType">The existing content type to update.</param>
    /// <param name="model">The update model to validate and map.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}"/> containing the updated content type on success,
    ///     or a <see cref="ContentTypeOperationStatus"/> indicating the validation failure reason.
    /// </returns>
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

    /// <summary>
    ///     Performs additional validation specific to the content type being created.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <returns>The validation status.</returns>
    /// <remarks>Override this method to add content-type-specific creation validation.</remarks>
    protected virtual Task<ContentTypeOperationStatus> AdditionalCreateValidationAsync(
        ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
        => Task.FromResult(ContentTypeOperationStatus.Success);

    #region Sanitization

    /// <summary>
    ///     Sanitizes all aliases in the model to ensure they are safe for use.
    /// </summary>
    /// <param name="model">The model containing aliases to sanitize.</param>
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

    /// <summary>
    ///     Performs comprehensive validation on the content type model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="contentType">The existing content type (for updates) or <c>null</c> (for creates).</param>
    /// <param name="allContentTypeCompositions">All existing content type compositions for validation.</param>
    /// <returns>The validation status.</returns>
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

    /// <summary>
    ///     Validates all aliases in the model for correctness and uniqueness.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <returns>The validation status.</returns>
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

    /// <summary>
    ///     Validates that all data types referenced by properties exist.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <returns>The validation status.</returns>
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

    /// <summary>
    ///     Validates inheritance and parent container relationships for a new content type.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="containerKey">The optional container key.</param>
    /// <returns>The validation status.</returns>
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

    /// <summary>
    ///     Validates inheritance and parent relationships for an existing content type.
    /// </summary>
    /// <param name="contentType">The existing content type.</param>
    /// <param name="model">The update model to validate.</param>
    /// <returns>The validation status.</returns>
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

    /// <summary>
    ///     Validates that the requested compositions are allowed.
    /// </summary>
    /// <param name="contentType">The existing content type (for updates) or <c>null</c> (for creates).</param>
    /// <param name="model">The model to validate.</param>
    /// <param name="allContentTypeCompositions">All existing content type compositions.</param>
    /// <returns>The validation status.</returns>
    private ContentTypeOperationStatus ValidateCompositions(TContentType? contentType, ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, IContentTypeComposition[] allContentTypeCompositions)
    {
        // get the content type keys we want to use for compositions
        Guid[] compositionKeys = KeysForCompositionTypes(model, CompositionType.Composition);

        // if the content type keys are already set as compositions, don't perform any additional validation
        // - this covers an edge case where compositions are configured for a content type before child content types are created
        if (contentType is not null && contentType.ContentTypeComposition
            .Select(c => c.Key)
            .ContainsAll(compositionKeys))
        {
            return ContentTypeOperationStatus.Success;
        }

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

    /// <summary>
    ///     Validates that all properties have unique aliases and don't conflict with compositions.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="allContentTypeCompositions">All existing content type compositions.</param>
    /// <returns>The validation status.</returns>
    private static ContentTypeOperationStatus ValidateProperties(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, IContentTypeComposition[] allContentTypeCompositions)
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

    /// <summary>
    ///     Validates that all containers (property groups/tabs) are properly configured.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="allContentTypeCompositions">All existing content type compositions.</param>
    /// <returns>The validation status.</returns>
    private static ContentTypeOperationStatus ValidateContainers(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, IContentTypeComposition[] allContentTypeCompositions)
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
    private bool ContentTypeAliasIsInUse(string alias) => _contentTypeService.GetAllContentTypeAliases().InvariantContains(alias);

    /// <summary>
    ///     Checks if an alias can be used for a specific content type.
    /// </summary>
    /// <param name="alias">The alias to check.</param>
    /// <param name="key">The key of the content type that wants to use the alias.</param>
    /// <returns><c>true</c> if the alias can be used; otherwise, <c>false</c>.</returns>
    private bool ContentTypeAliasCanBeUsedFor(string alias, Guid key)
    {
        IContentType? existingContentType = _contentTypeService.Get(alias);
        if (existingContentType is null || existingContentType.Key == key)
        {
            return true;
        }

        return ContentTypeAliasIsInUse(alias) is false;
    }

    /// <summary>
    ///     Checks if an alias is reserved and cannot be used for content types.
    /// </summary>
    /// <param name="alias">The alias to check.</param>
    /// <returns><c>true</c> if the alias is reserved; otherwise, <c>false</c>.</returns>
    private static bool IsReservedContentTypeAlias(string alias)
    {
        var reservedAliases = new[] { "system" };
        return reservedAliases.InvariantContains(alias);
    }

    /// <summary>
    ///     Gets the set of reserved field names that cannot be used as property aliases.
    /// </summary>
    /// <returns>The set of reserved field names.</returns>
    protected abstract ISet<string> GetReservedFieldNames();

    /// <summary>
    ///     Checks if the model contains any reserved property type aliases.
    /// </summary>
    /// <param name="model">The model to check.</param>
    /// <returns><c>true</c> if any property uses a reserved alias; otherwise, <c>false</c>.</returns>
    private bool ContainsReservedPropertyTypeAlias(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        // Because of models builder you cannot have an alias that already exists in IPublishedContent, for instance Path.
        // Since MyModel.Path would conflict with IPublishedContent.Path.
        ISet<string> reservedPropertyTypeNames = GetReservedFieldNames();

        return model.Properties.Any(propertyType => reservedPropertyTypeNames.InvariantContains(propertyType.Alias));
    }

    /// <summary>
    ///     Checks if an alias is unsafe (empty, whitespace, or contains invalid characters).
    /// </summary>
    /// <param name="alias">The alias to check.</param>
    /// <returns><c>true</c> if the alias is unsafe; otherwise, <c>false</c>.</returns>
    private bool IsUnsafeAlias(string alias) => alias.IsNullOrWhiteSpace()
                                                || alias.Length != alias.ToSafeAlias(_shortStringHelper).Length;

    #endregion

    #region Model update

    /// <summary>
    ///     Updates a content type with values from the model.
    /// </summary>
    /// <param name="contentType">The content type to update.</param>
    /// <param name="model">The model containing the new values.</param>
    /// <param name="allContentTypeCompositions">All existing content type compositions.</param>
    /// <returns>The updated content type.</returns>
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

        // update/map all properties
        await UpdatePropertiesAsync(contentType, model);

        // update the allowed content types
        UpdateAllowedContentTypes(contentType, model, allContentTypeCompositions);

        // update all compositions
        UpdateCompositions(contentType, model, allContentTypeCompositions);

        // ensure parent content type assignment (inheritance) if any
        UpdateParentContentType(contentType, model, allContentTypeCompositions);

        return contentType;
    }

    /// <summary>
    ///     Updates the allowed child content types for a content type.
    /// </summary>
    /// <param name="contentType">The content type to update.</param>
    /// <param name="model">The model containing the allowed content types.</param>
    /// <param name="allContentTypeCompositions">All existing content type compositions.</param>
    private static void UpdateAllowedContentTypes(
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

    /// <summary>
    ///     Updates the properties and property groups for a content type.
    /// </summary>
    /// <param name="contentType">The content type to update.</param>
    /// <param name="model">The model containing the property definitions.</param>
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

        // Store the existing property types in a list to reference when processing properties.
        // This ensures we correctly handle property types that may have been filtered out from groups.
        var existingPropertyTypes = contentType.PropertyTypes.ToList();

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
                    .Select(property => MapProperty(contentType, property, propertyGroup, existingPropertyTypes, dataTypesByKey))
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
        IEnumerable<TPropertyTypeModel> orphanedPropertyTypeModels = model.Properties.Where(x => x.ContainerKey is null).ToArray();
        IPropertyType[] orphanedPropertyTypes = orphanedPropertyTypeModels.Select(property => MapProperty(contentType, property, null, existingPropertyTypes, dataTypesByKey)).ToArray();
        if (contentType.NoGroupPropertyTypes.SequenceEqual(orphanedPropertyTypes) is false)
        {
            contentType.NoGroupPropertyTypes = new PropertyTypeCollection(SupportsPublishing, orphanedPropertyTypes);
        }
    }

    /// <summary>
    ///     Generates a property group alias from a container name.
    /// </summary>
    /// <param name="containerName">The container name to convert.</param>
    /// <returns>The generated alias in camelCase format.</returns>
    private static string PropertyGroupAlias(string? containerName)
    {
        if (containerName.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Container name cannot be empty", nameof(containerName));
        }

        var parts = containerName.Split(Constants.CharArrays.Space);
        return $"{parts.First().ToFirstLowerInvariant()}{string.Join(string.Empty, parts.Skip(1).Select(part => part.ToFirstUpperInvariant()))}";
    }

    /// <summary>
    ///     Maps a property model to an <see cref="IPropertyType"/>.
    /// </summary>
    /// <param name="contentType">The content type the property belongs to.</param>
    /// <param name="property">The property model to map.</param>
    /// <param name="propertyGroup">The property group the property belongs to, or <c>null</c> for ungrouped properties.</param>
    /// <param name="existingPropertyTypes">Existing property types for reference.</param>
    /// <param name="dataTypesByKey">Dictionary of data types keyed by their unique identifier.</param>
    /// <returns>The mapped property type.</returns>
    private IPropertyType MapProperty(
        TContentType contentType,
        TPropertyTypeModel property,
        PropertyGroup? propertyGroup,
        IEnumerable<IPropertyType> existingPropertyTypes,
        IDictionary<Guid, IDataType> dataTypesByKey)
    {
        // get the selected data type
        // NOTE: this only works because we already ensured that the data type is present in the dataTypesByKey dictionary
        if (dataTypesByKey.TryGetValue(property.DataTypeKey, out IDataType? dataType) is false)
        {
            throw new ArgumentException("One or more data types could not be found", nameof(dataTypesByKey));
        }

        // get the current property type (if it exists)
        IPropertyType propertyType = existingPropertyTypes.FirstOrDefault(pt => pt.Key == property.Key)
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

        propertyType.PropertyGroupId = propertyGroup is null
            ? null
            : new Lazy<int>(() => propertyGroup.Id, false);

        return propertyType;
    }

    /// <summary>
    ///     Updates the compositions for a content type.
    /// </summary>
    /// <param name="contentType">The content type to update.</param>
    /// <param name="model">The model containing the composition definitions.</param>
    /// <param name="allContentTypeCompositions">All existing content type compositions.</param>
    private static void UpdateCompositions(
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

    /// <summary>
    ///     Updates the parent content type (inheritance) for a content type.
    /// </summary>
    /// <param name="contentType">The content type to update.</param>
    /// <param name="model">The model containing the inheritance definition.</param>
    /// <param name="allContentTypeCompositions">All existing content type compositions.</param>
    private static void UpdateParentContentType(
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

    /// <summary>
    ///     Gets the unique data type keys from a model.
    /// </summary>
    /// <param name="model">The model to extract keys from.</param>
    /// <returns>An array of unique data type keys.</returns>
    private static Guid[] GetDataTypeKeys(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
        => model.Properties.Select(property => property.DataTypeKey).Distinct().ToArray();

    /// <summary>
    ///     Gets the data types referenced by properties in the model.
    /// </summary>
    /// <param name="model">The model containing property definitions.</param>
    /// <returns>An array of data types.</returns>
    private async Task<IDataType[]> GetDataTypesAsync(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model)
    {
        Guid[] dataTypeKeys = GetDataTypeKeys(model);
        return dataTypeKeys.Any()
            ? (await _dataTypeService.GetAllAsync(GetDataTypeKeys(model))).ToArray()
            : [];
    }

    /// <summary>
    ///     Gets the parent ID for a content type based on inheritance or container placement.
    /// </summary>
    /// <param name="model">The model containing composition definitions.</param>
    /// <param name="containerKey">The optional container key.</param>
    /// <returns>The parent ID, or <c>null</c> if no valid parent could be determined.</returns>
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

    /// <summary>
    ///     Gets the keys of compositions matching the specified composition types.
    /// </summary>
    /// <param name="model">The model containing composition definitions.</param>
    /// <param name="compositionTypes">The composition types to filter by.</param>
    /// <returns>An array of composition keys.</returns>
    private static Guid[] KeysForCompositionTypes(ContentTypeEditingModelBase<TPropertyTypeModel, TPropertyTypeContainer> model, params CompositionType[] compositionTypes)
        => model.Compositions
            .Where(c => compositionTypes.Contains(c.CompositionType))
            .Select(c => c.Key)
            .ToArray();

    /// <summary>
    ///     Gets all content type compositions from the service.
    /// </summary>
    /// <returns>An array of all content type compositions.</returns>
    private IContentTypeComposition[] GetAllContentTypeCompositions()
        // NOTE: using Cast here is OK, because we implicitly enforce the constraint TContentType : IContentTypeComposition
        => _concreteContentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();

    #endregion
}

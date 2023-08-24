using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal abstract class ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>
    where TContent : class, IContentBase
    where TContentType : class, IContentTypeComposition
    where TContentService : IContentServiceBase<TContent>
    where TContentTypeService : IContentTypeBaseService<TContentType>
{
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IDataTypeService _dataTypeService;
    private readonly ILogger<ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> _logger;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ITreeEntitySortingService _treeEntitySortingService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    protected ContentEditingServiceBase(
        TContentService contentService,
        TContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        ITreeEntitySortingService treeEntitySortingService)
    {
        _propertyEditorCollection = propertyEditorCollection;
        _dataTypeService = dataTypeService;
        _logger = logger;
        _scopeProvider = scopeProvider;
        _userIdKeyResolver = userIdKeyResolver;
        _treeEntitySortingService = treeEntitySortingService;
        ContentService = contentService;
        ContentTypeService = contentTypeService;
    }

    protected abstract TContent New(string? name, int parentId, TContentType contentType);

    protected abstract OperationResult? Move(TContent content, int newParentId, int userId);

    protected abstract TContent? Copy(TContent content, int newParentId, bool relateToOriginal, bool includeDescendants, int userId);

    protected abstract OperationResult? MoveToRecycleBin(TContent content, int userId);

    protected abstract OperationResult? Delete(TContent content, int userId);

    protected abstract IEnumerable<TContent> GetPagedChildren(int parentId, int pageIndex, int pageSize, out long total);

    protected abstract ContentEditingOperationStatus Sort(IEnumerable<TContent> items, int userId);

    protected TContentService ContentService { get; }

    protected TContentTypeService ContentTypeService { get; }

    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> MapCreate(ContentCreationModelBase contentCreationModelBase)
    {
        TContentType? contentType = TryGetAndValidateContentType(contentCreationModelBase.ContentTypeKey, contentCreationModelBase, out ContentEditingOperationStatus operationStatus);
        if (contentType == null)
        {
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(operationStatus, null);
        }

        TContent? parent = TryGetAndValidateParent(contentCreationModelBase.ParentKey, contentType, out operationStatus);
        if (operationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(operationStatus, null);
        }

        TContent content = New(null, parent?.Id ?? Constants.System.Root, contentType);

        UpdateNames(contentCreationModelBase, content, contentType);
        await UpdateExistingProperties(contentCreationModelBase, content, contentType);

        return Attempt.SucceedWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, content);
    }

    protected async Task<Attempt<ContentEditingOperationStatus>> MapUpdate(TContent content, ContentEditingModelBase contentEditingModelBase)
    {
        TContentType? contentType = TryGetAndValidateContentType(content.ContentType.Key, contentEditingModelBase, out ContentEditingOperationStatus operationStatus);
        if (contentType == null)
        {
            return Attempt.Fail(operationStatus);
        }

        UpdateNames(contentEditingModelBase, content, contentType);
        await UpdateExistingProperties(contentEditingModelBase, content, contentType);
        RemoveMissingProperties(contentEditingModelBase, content, contentType);

        return Attempt.Succeed(ContentEditingOperationStatus.Success);
    }

    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleMoveToRecycleBinAsync(Guid key, Guid userKey)
        => await HandleDeletionAsync(key, userKey, false, MoveToRecycleBin);

    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleDeleteAsync(Guid key, Guid userKey)
        => await HandleDeletionAsync(key, userKey, true, Delete);

    // helper method to perform move-to-recycle-bin and delete for content as they are very much handled in the same way
    private async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleDeletionAsync(Guid key, Guid userKey, bool mustBeTrashed, Func<TContent, int, OperationResult?> performDelete)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        TContent? content = ContentService.GetById(key);
        if (content == null)
        {
            return await Task.FromResult(Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, content));
        }

        if (content.Trashed != mustBeTrashed)
        {
            ContentEditingOperationStatus status = mustBeTrashed
                ? ContentEditingOperationStatus.NotInTrash
                : ContentEditingOperationStatus.InTrash;
            return await Task.FromResult(Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(status, content));
        }

        var userId = await GetUserIdAsync(userKey);
        OperationResult? deleteResult = performDelete(content, userId);

        scope.Complete();

        return OperationResultToAttempt(content, deleteResult);
    }

    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleMoveAsync(Guid key, Guid? parentKey, Guid userKey)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        TContent? content = ContentService.GetById(key);
        if (content is null)
        {
            return await Task.FromResult(Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, content));
        }

        TContentType contentType = ContentTypeService.Get(content.ContentType.Key)!;

        TContent? parent = TryGetAndValidateParent(parentKey, contentType, out ContentEditingOperationStatus operationStatus);
        if (operationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(operationStatus, content);
        }

        // special case for move: short circuit the operation if the content is already under the correct parent.
        if ((parent == null && content.ParentId == Constants.System.Root) || (parent != null && parent.Id == content.ParentId))
        {
            return Attempt.SucceedWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, content);
        }

        // special case for move: do not allow moving an item beneath itself.
        if (parent?.Path.Split(Constants.CharArrays.Comma).Select(int.Parse).Contains(content.Id) is true)
        {
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.ParentInvalid, content);
        }

        var userId = await GetUserIdAsync(userKey);
        OperationResult? moveResult = Move(content, parent?.Id ?? Constants.System.Root, userId);

        scope.Complete();

        return OperationResultToAttempt(content, moveResult);
    }

    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleCopyAsync(Guid key, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        TContent? content = ContentService.GetById(key);
        if (content is null)
        {
            return await Task.FromResult(Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, content));
        }

        TContentType contentType = ContentTypeService.Get(content.ContentType.Key)!;

        TContent? parent = TryGetAndValidateParent(parentKey, contentType, out ContentEditingOperationStatus operationStatus);
        if (operationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(operationStatus, content);
        }

        var userId = await GetUserIdAsync(userKey);
        TContent? copy = Copy(content, parent?.Id ?? Constants.System.Root, relateToOriginal, includeDescendants, userId);
        scope.Complete();

        // we'll assume that we have performed all validations for unsuccessful scenarios above, so a null result here
        // means the copy operation was cancelled by a notification handler
        return copy != null
            ? Attempt.SucceedWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, copy)
            : Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.CancelledByNotification, null);
    }

    protected async Task<ContentEditingOperationStatus> HandleSortAsync(
        Guid? parentKey,
        IEnumerable<SortingModel> sortingModels,
        Guid userKey)
    {
        var contentId = parentKey.HasValue
            ? ContentService.GetById(parentKey.Value)?.Id
            : Constants.System.Root;

        if (contentId.HasValue is false)
        {
            return await Task.FromResult(ContentEditingOperationStatus.NotFound);
        }

        const int pageSize = 500;
        var pageNumber = 0;
        IEnumerable<TContent> page = GetPagedChildren(contentId.Value, pageNumber++, pageSize, out var total);
        var children = new List<TContent>((int)total);
        children.AddRange(page);
        while (pageNumber * pageSize < total)
        {
            page = GetPagedChildren(contentId.Value, pageNumber++, pageSize, out _);
            children.AddRange(page);
        }

        try
        {
            TContent[] sortedChildren = _treeEntitySortingService
                .SortEntities(children, sortingModels)
                .ToArray();

            var userId = await GetUserIdAsync(userKey);

            return Sort(sortedChildren, userId);
        }
        catch (ArgumentException argumentException)
        {
            _logger.LogError(argumentException, "Invalid sorting instructions, see exception for details.");
            return ContentEditingOperationStatus.SortingInvalid;
        }
    }

    private Attempt<TContent?, ContentEditingOperationStatus> OperationResultToAttempt(TContent? content, OperationResult? operationResult)
    {
        ContentEditingOperationStatus operationStatus = OperationResultToOperationStatus(operationResult);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(operationStatus, content)
            : Attempt.FailWithStatus(operationStatus, content);
    }

    protected ContentEditingOperationStatus OperationResultToOperationStatus(OperationResult? operationResult)
        => operationResult?.Result switch
        {
            // these are the only result states currently expected from the invoked IContentService operations
            OperationResultType.Success => ContentEditingOperationStatus.Success,
            OperationResultType.FailedCancelledByEvent => ContentEditingOperationStatus.CancelledByNotification,

            // for any other state we'll return "unknown" so we know that we need to amend this switch statement
            _ => ContentEditingOperationStatus.Unknown
        };

    protected async Task<int> GetUserIdAsync(Guid userKey) => await _userIdKeyResolver.GetAsync(userKey);

    private TContentType? TryGetAndValidateContentType(Guid contentTypeKey, ContentEditingModelBase contentEditingModelBase, out ContentEditingOperationStatus operationStatus)
    {
        TContentType? contentType = ContentTypeService.Get(contentTypeKey);
        if (contentType == null)
        {
            operationStatus = ContentEditingOperationStatus.ContentTypeNotFound;
            return null;
        }

        if (contentType.VariesByCulture() == false)
        {
            if (contentEditingModelBase.InvariantName.IsNullOrWhiteSpace() || contentEditingModelBase.Variants.Any())
            {
                operationStatus = ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch;
                return null;
            }
        }

        var propertyTypesByAlias = contentType.CompositionPropertyTypes.ToDictionary(pt => pt.Alias);
        var propertyValuesAndVariance = contentEditingModelBase
            .InvariantProperties
            .Select(pv => new
            {
                VariesByCulture = false,
                VariesBySegment = false,
                PropertyValue = pv
            })
            .Union(contentEditingModelBase
                .Variants
                .SelectMany(v => v
                    .Properties
                    .Select(vpv => new
                    {
                        VariesByCulture = true,
                        VariesBySegment = v.Segment.IsNullOrWhiteSpace() == false,
                        PropertyValue = vpv
                    })))
            .ToArray();

        // verify that all property values are defined as property types
        if (propertyValuesAndVariance.Any(pv => propertyTypesByAlias.ContainsKey(pv.PropertyValue.Alias) == false))
        {
            operationStatus = ContentEditingOperationStatus.PropertyTypeNotFound;
            return null;
        }

        // verify that all properties match their respective property type culture and segment variance - i.e. no culture invariant properties that should have been culture variant
        if (propertyValuesAndVariance.Any(pv =>
            {
                IPropertyType propertyType = propertyTypesByAlias[pv.PropertyValue.Alias];
                return propertyType.VariesByCulture() != pv.VariesByCulture || propertyType.VariesBySegment() != pv.VariesBySegment;
            }))
        {
            operationStatus = ContentEditingOperationStatus.PropertyTypeNotFound;
            return null;
        }

        operationStatus = ContentEditingOperationStatus.Success;
        return contentType;
    }

    private TContent? TryGetAndValidateParent(Guid? parentKey, TContentType contentType, out ContentEditingOperationStatus operationStatus)
    {
        TContent? parent = parentKey.HasValue
            ? ContentService.GetById(parentKey.Value)
            : null;

        if (parentKey.HasValue && parent == null)
        {
            operationStatus = ContentEditingOperationStatus.ParentNotFound;
            return null;
        }

        if (parent == null && contentType.AllowedAsRoot == false)
        {
            operationStatus = ContentEditingOperationStatus.NotAllowed;
            return null;
        }

        if (parent != null)
        {
            if (parent.Trashed)
            {
                operationStatus = ContentEditingOperationStatus.InTrash;
                return null;
            }

            TContentType? parentContentType = ContentTypeService.Get(parent.ContentType.Key);
            Guid[] allowedContentTypeKeys = parentContentType?.AllowedContentTypes?.Select(c => c.Key).ToArray()
                                            ?? Array.Empty<Guid>();

            if (allowedContentTypeKeys.Contains(contentType.Key) == false)
            {
                operationStatus = ContentEditingOperationStatus.NotAllowed;
                return null;
            }
        }

        operationStatus = ContentEditingOperationStatus.Success;
        return parent;
    }

    private void UpdateNames(ContentEditingModelBase contentEditingModelBase, TContent content, TContentType contentType)
    {
        if (contentType.VariesByCulture())
        {
            // get the content names for each culture, keeping in mind that there may be multiple per culture
            // as each culture can have several segments. we'll prioritize the segment-less names
            var variantNamesByCulture = contentEditingModelBase.Variants
                .Where(v => v.Culture.IsNullOrWhiteSpace() == false)
                .OrderBy(v => v.Segment.IsNullOrWhiteSpace() ? 0 : 1)
                .GroupBy(v => v.Culture!)
                .ToDictionary(g => g.Key, g => g.First().Name);

            // update the content names for all cultures
            foreach (var (culture, name) in variantNamesByCulture)
            {
                content.SetCultureName(name, culture);
            }
        }
        else
        {
            // this should be validated already so it's OK to throw an exception here
            content.Name = contentEditingModelBase.InvariantName
                           ?? throw new ArgumentException("Could not find a culture invariant variant", nameof(contentEditingModelBase));
        }
    }

    private async Task UpdateExistingProperties(ContentEditingModelBase contentEditingModelBase, TContent content, TContentType contentType)
    {
        // create a mapping dictionary for all content type property types by their property aliases
        Dictionary<string, IPropertyType> propertyTypesByAlias = GetPropertyTypesByAlias(contentType);

        // flatten the invariant and variant property values from the model into one array, and remove any properties
        // that do not exist on the content type
        var propertyValues = contentEditingModelBase
            .InvariantProperties
            .Select(pv => new { Culture = (string?)null, Segment = (string?)null, Alias = pv.Alias, Value = pv.Value })
            .Union(contentEditingModelBase
                .Variants
                .SelectMany(v => v
                    .Properties
                    .Select(vpv => new { Culture = v.Culture, Segment = v.Segment, Alias = vpv.Alias, Value = vpv.Value })))
            .Where(propertyValue => propertyTypesByAlias.ContainsKey(propertyValue.Alias))
            .ToArray();

        // update all properties on the content item
        foreach (var propertyValue in propertyValues)
        {
            // the following checks should already have been validated by now, so it's OK to throw exceptions here
            if(propertyTypesByAlias.TryGetValue(propertyValue.Alias, out IPropertyType? propertyType) == false
               || (propertyType.VariesByCulture() && propertyValue.Culture.IsNullOrWhiteSpace())
               || (propertyType.VariesBySegment() && propertyValue.Segment.IsNullOrWhiteSpace()))
            {
                throw new ArgumentException($"Culture or segment variance mismatch for property: {propertyValue.Alias}", nameof(contentEditingModelBase));
            }

            // pass the value through the data editor to construct the value to store in the content
            var dataEditorValue = await GetDataEditorValue(propertyValue.Value, propertyValue.Culture, propertyValue.Segment, propertyType, content);
            content.SetValue(propertyValue.Alias, dataEditorValue, propertyValue.Culture, propertyValue.Segment);
        }
    }

    private void RemoveMissingProperties(ContentEditingModelBase contentEditingModelBase, TContent content, TContentType contentType)
    {
        // create a mapping dictionary for all content type property types by their property aliases
        Dictionary<string, IPropertyType> propertyTypesByAlias = GetPropertyTypesByAlias(contentType);
        var knownPropertyAliases = contentEditingModelBase
            .InvariantProperties.Select(pv => pv.Alias)
            .Union(contentEditingModelBase.Variants.SelectMany(v => v.Properties.Select(vpv => vpv.Alias)))
            .Distinct()
            .ToArray();

        var missingPropertyAliases = propertyTypesByAlias.Keys.Except(knownPropertyAliases).ToArray();
        foreach (var propertyAlias in missingPropertyAliases)
        {
            content.RemoveValue(propertyAlias);
        }
    }

    private async Task<object?> GetDataEditorValue(object? value, string? culture, string? segment, IPropertyType propertyType, TContent content)
    {
        // this should already have been validated by now, so it's OK to throw exceptions here
        if (_propertyEditorCollection.TryGet(propertyType.PropertyEditorAlias, out IDataEditor? dataEditor) == false)
        {
            _logger.LogWarning("Unable to retrieve property value - no data editor found for property editor: {PropertyEditorAlias}", propertyType.PropertyEditorAlias);
            return null;
        }

        IDataValueEditor dataValueEditor = dataEditor.GetValueEditor();
        if (dataValueEditor.IsReadOnly)
        {
            return null;
        }

        IDataType? dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey);
        var data = new ContentPropertyData(value, dataType?.ConfigurationObject)
        {
            ContentKey = content.Key,
            PropertyTypeKey = propertyType.Key
        };

        var currentValue = content.GetValue(propertyType.Alias, culture, segment);
        return dataValueEditor.FromEditor(data, currentValue);
    }

    private static Dictionary<string, IPropertyType> GetPropertyTypesByAlias(TContentType contentType)
        => contentType.CompositionPropertyTypes.ToDictionary(pt => pt.Alias);
}

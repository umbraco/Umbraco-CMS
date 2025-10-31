using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

public class MigrateSingleBlockList : AsyncMigrationBase
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly ILanguageService _languageService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly SingleBlockListProcessor _singleBlockListProcessor;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly SingleBlockListConfigurationCache _blockListConfigurationCache;
    private readonly IBlockEditorElementTypeCache _elementTypeCache;
    private readonly AppCaches _appCaches;
    private readonly ILogger<MigrateSingleBlockList> _logger;
    private readonly IDataValueEditor _dummySingleBlockValueEditor;

    public MigrateSingleBlockList(
        IMigrationContext context,
        IUmbracoContextFactory umbracoContextFactory,
        ILanguageService languageService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        ILogger<MigrateSingleBlockList> logger,
        ICoreScopeProvider coreScopeProvider,
        SingleBlockListProcessor  singleBlockListProcessor,
        IJsonSerializer  jsonSerializer,
        SingleBlockListConfigurationCache blockListConfigurationCache,
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory,
        IBlockEditorElementTypeCache elementTypeCache,
        AppCaches appCaches)
        : base(context)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _languageService = languageService;
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _dataTypeService = dataTypeService;
        _logger = logger;
        _coreScopeProvider = coreScopeProvider;
        _singleBlockListProcessor = singleBlockListProcessor;
        _jsonSerializer = jsonSerializer;
        _blockListConfigurationCache = blockListConfigurationCache;
        _elementTypeCache = elementTypeCache;
        _appCaches = appCaches;

        _dummySingleBlockValueEditor = new SingleBlockPropertyEditor(dataValueEditorFactory, jsonSerializer, ioHelper, blockValuePropertyIndexValueFactory).GetValueEditor();
    }

    protected override async Task MigrateAsync()
    {
        IEnumerable<string> propertyEditorAliases = _singleBlockListProcessor.GetSupportedPropertyEditorAliases();

        using UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();
        var languagesById = (await _languageService.GetAllAsync())
            .ToDictionary(language => language.Id);

        IEnumerable<IContentType> allContentTypes = _contentTypeService.GetAll();
        IEnumerable<IPropertyType> contentPropertyTypes = allContentTypes
            .SelectMany(ct => ct.PropertyTypes);

        IMediaType[] allMediaTypes = _mediaTypeService.GetAll().ToArray();
        IEnumerable<IPropertyType> mediaPropertyTypes = allMediaTypes
            .SelectMany(ct => ct.PropertyTypes);

        // get all relevantPropertyTypes
        var relevantPropertyEditors =
            contentPropertyTypes.Concat(mediaPropertyTypes).DistinctBy(pt => pt.Id)
                .Where(pt => propertyEditorAliases.Contains(pt.PropertyEditorAlias))
                .GroupBy(pt => pt.PropertyEditorAlias)
                .ToDictionary(group => group.Key, group => group.ToArray());

        var blockListsConfiguredAsSingleCount = await _blockListConfigurationCache.Populate();

        if (blockListsConfiguredAsSingleCount == 0)
        {
            _logger.LogInformation(
                "No blocklist were configured as single, nothing to do.");
            return;
        }

        _logger.LogInformation(
            "Found {blockListsConfiguredAsSingleCount} number of blockListConfigurations with UseSingleBlockMode set to true",
            blockListsConfiguredAsSingleCount);

        var updateItemsByPropertyEditorAlias = new Dictionary<string, Dictionary<IPropertyType, List<UpdateItem>>>();

        // update all propertyData for each propertyType
        foreach (var propertyEditorAlias in propertyEditorAliases)
        {
            if (relevantPropertyEditors.TryGetValue(propertyEditorAlias, out IPropertyType[]? propertyTypes) is false)
            {
                continue;
            }

            _logger.LogInformation(
                "Migration starting for all properties of type: {propertyEditorAlias}",
                propertyEditorAlias);
            Dictionary<IPropertyType, List<UpdateItem>> updateItemsByPropertyType = await ProcessPropertyTypesAsync(propertyTypes, languagesById);
            if (updateItemsByPropertyType.Count < 1)
            {
                _logger.LogInformation(
                    "No properties have been found to migrate for {propertyEditorAlias}",
                    propertyEditorAlias);
                return;
            }

            updateItemsByPropertyEditorAlias[propertyEditorAlias] = updateItemsByPropertyType;
        }

        // update the configuration of all propertyTypes
        var singleBlockListDataTypesIds = _blockListConfigurationCache.CachedDataTypes.ToList().Select(type => type.Id).ToList();

        string updateSql = $@"
UPDATE umbracoDataType
SET propertyEditorAlias = '{Constants.PropertyEditors.Aliases.SingleBlock}',
    propertyEditorUiAlias = 'Umb.PropertyEditorUi.SingleBlock'
WHERE nodeId IN (@0)";
        await Database.ExecuteAsync(updateSql, singleBlockListDataTypesIds);

        // we need to clear the elementTypeCache so the second part of the migration can work with the update dataTypes
        // and also the isolated/runtime Caches as that is what its build from in the default implementation
        _elementTypeCache.ClearAll();
        _appCaches.IsolatedCaches.ClearAllCaches();
        _appCaches.RuntimeCache.Clear();
        RebuildCache = true;

        foreach (string propertyEditorAlias in updateItemsByPropertyEditorAlias.Keys)
        {
            if (await SavePropertyTypes(updateItemsByPropertyEditorAlias[propertyEditorAlias]))
            {
                _logger.LogInformation(
                    "Migration succeeded for all properties of type: {propertyEditorAlias}",
                    propertyEditorAlias);
            }
            else
            {
                _logger.LogError(
                    "Migration failed for one or more properties of type: {propertyEditorAlias}",
                    propertyEditorAlias);
            }
        }
    }

    private async Task<Dictionary<IPropertyType, List<UpdateItem>>> ProcessPropertyTypesAsync(IPropertyType[] propertyTypes, IDictionary<int, ILanguage> languagesById)
    {
        var updateItemsByPropertyType = new Dictionary<IPropertyType, List<UpdateItem>>();
        foreach (IPropertyType propertyType in propertyTypes)
        {
            IDataType dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey)
                                 ?? throw new InvalidOperationException("The data type could not be fetched.");

            IDataValueEditor valueEditor = dataType.Editor?.GetValueEditor()
                                           ?? throw new InvalidOperationException(
                                               "The data type value editor could not be obtained.");

            Sql<ISqlContext> sql = Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .InnerJoin<ContentVersionDto>()
                .On<PropertyDataDto, ContentVersionDto>((propertyData, contentVersion) =>
                    propertyData.VersionId == contentVersion.Id)
                .LeftJoin<DocumentVersionDto>()
                .On<ContentVersionDto, DocumentVersionDto>((contentVersion, documentVersion) =>
                    contentVersion.Id == documentVersion.Id)
                .Where<PropertyDataDto, ContentVersionDto, DocumentVersionDto>((propertyData, contentVersion, documentVersion) =>
                    (contentVersion.Current == true || documentVersion.Published == true)
                    && propertyData.PropertyTypeId == propertyType.Id);

            List<PropertyDataDto> propertyDataDtos = await Database.FetchAsync<PropertyDataDto>(sql);
            if (propertyDataDtos.Count < 1)
            {
                continue;
            }

            var updateItems = new List<UpdateItem>();

            foreach (PropertyDataDto propertyDataDto in propertyDataDtos)
            {
                if (ProcessPropertyDataDto(propertyDataDto, propertyType, languagesById, valueEditor, out UpdateItem? updateItem) is false)
                {
                    continue;
                }

                updateItems.Add(updateItem!);
            }

            updateItemsByPropertyType[propertyType] = updateItems;
        }

        return updateItemsByPropertyType;
    }

    private async Task<bool> SavePropertyTypes(IDictionary<IPropertyType, List<UpdateItem>> propertyTypes)
    {
        foreach (IPropertyType propertyType in propertyTypes.Keys)
        {
            IDataType dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey)
                                 ?? throw new InvalidOperationException("The data type could not be fetched.");

            IDataValueEditor updatedValueEditor = dataType.Editor?.GetValueEditor()
                                           ?? throw new InvalidOperationException(
                                               "The data type value editor could not be obtained.");

            var propertyDataDtos = propertyTypes[propertyType].Select(item => item.PropertyDataDto).ToList();

            var updateBatch = propertyDataDtos.Select(propertyDataDto =>
                UpdateBatch.For(propertyDataDto, Database.StartSnapshot(propertyDataDto))).ToList();

            var updatesToSkip = new ConcurrentBag<UpdateBatch<PropertyDataDto>>();

            var progress = 0;

            void HandleUpdateBatch(UpdateBatch<PropertyDataDto> update)
            {
                using UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

                progress++;
                if (progress % 100 == 0)
                {
                    _logger.LogInformation("  - finíshed {progress} of {total} properties", progress, updateBatch.Count);
                }

                PropertyDataDto propertyDataDto = update.Poco;

                if (FinalizeUpdateItem(propertyTypes[propertyType].First(item => Equals(item.PropertyDataDto, update.Poco)), updatedValueEditor) is false)
                {
                    updatesToSkip.Add(update);
                }
            }

            if (DatabaseType == DatabaseType.SQLite)
            {
                // SQLite locks up if we run the migration in parallel, so... let's not.
                foreach (UpdateBatch<PropertyDataDto> update in updateBatch)
                {
                    HandleUpdateBatch(update);
                }
            }
            else
            {
                Parallel.ForEachAsync(updateBatch, async (update, token) =>
                {
                    //Foreach here, but we need to suppress the flow before each task, but not the actuall await of the task
                    Task task;
                    using (ExecutionContext.SuppressFlow())
                    {
                        task = Task.Run(
                            () =>
                            {
                                using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
                                scope.Complete();
                                HandleUpdateBatch(update);
                            },
                            token);
                    }

                    await task;
                }).GetAwaiter().GetResult();
            }

            updateBatch.RemoveAll(updatesToSkip.Contains);

            if (updateBatch.Any() is false)
            {
                _logger.LogDebug("  - no properties to convert, continuing");
                continue;
            }

            _logger.LogInformation("  - {totalConverted} properties converted, saving...", updateBatch.Count);
            var result = Database.UpdateBatch(updateBatch, new BatchOptions { BatchSize = 100 });
            if (result != updateBatch.Count)
            {
                throw new InvalidOperationException(
                    $"The database batch update was supposed to update {updateBatch.Count} property DTO entries, but it updated {result} entries.");
            }

            _logger.LogDebug(
                "Migration completed for property type: {propertyTypeName} (id: {propertyTypeId}, alias: {propertyTypeAlias}, editor alias: {propertyTypeEditorAlias}) - {updateCount} property DTO entries updated.",
                propertyType.Name,
                propertyType.Id,
                propertyType.Alias,
                propertyType.PropertyEditorAlias,
                result);
        }

        return true;
    }

    private bool ProcessPropertyDataDto(
        PropertyDataDto propertyDataDto,
        IPropertyType propertyType,
        IDictionary<int, ILanguage> languagesById,
        IDataValueEditor valueEditor,
        out UpdateItem? updateItem)
    {
        // NOTE: some old property data DTOs can have variance defined, even if the property type no longer varies
        var culture = propertyType.VariesByCulture()
                      && propertyDataDto.LanguageId.HasValue
                      && languagesById.TryGetValue(propertyDataDto.LanguageId.Value, out ILanguage? language)
            ? language.IsoCode
            : null;

        if (culture is null && propertyType.VariesByCulture())
        {
            // if we end up here, the property DTO is bound to a language that no longer exists. this is an error scenario,
            // and we can't really handle it in any other way than logging; in all likelihood this is an old property version,
            // and it won't cause any runtime issues
            _logger.LogWarning(
                "    - property data with id: {propertyDataId} references a language that does not exist - language id: {languageId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                propertyDataDto.Id,
                propertyDataDto.LanguageId,
                propertyType.Name,
                propertyType.Id,
                propertyType.Alias);
            updateItem = null;
            return false;
        }

        var segment = propertyType.VariesBySegment() ? propertyDataDto.Segment : null;
        var property = new Property(propertyType);
        property.SetValue(propertyDataDto.Value, culture, segment);
        var toEditorValue = valueEditor.ToEditor(property, culture, segment);

        if (TryTransformValue(toEditorValue, property, out var updatedValue) is false)
        {
            _logger.LogDebug(
                "    - skipping as no processor modified the data for property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                propertyDataDto.Id,
                propertyType.Name,
                propertyType.Id,
                propertyType.Alias);
            updateItem = null;
            return false;
        }

        updateItem = new UpdateItem(propertyDataDto, propertyType, updatedValue);
        return true;
    }

    private bool FinalizeUpdateItem(UpdateItem updateItem, IDataValueEditor updatedValueEditor)
    {

        var editorValue = _jsonSerializer.Serialize(updateItem.UpdatedValue);
        var dbValue = updateItem.UpdatedValue is SingleBlockValue
            ? _dummySingleBlockValueEditor.FromEditor(new ContentPropertyData(editorValue, null), null)
            : updatedValueEditor.FromEditor(new ContentPropertyData(editorValue, null), null);
        if (dbValue is not string stringValue || stringValue.DetectIsJson() is false)
        {
            _logger.LogWarning(
                "    - value editor did not yield a valid JSON string as FromEditor value property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                updateItem.PropertyDataDto.Id,
                updateItem.PropertyType.Name,
                updateItem.PropertyType.Id,
                updateItem.PropertyType.Alias);
            return false;
        }

        updateItem.PropertyDataDto.TextValue = stringValue;
        return true;
    }

    private bool TryTransformValue(object? toEditorValue, Property property, out object? value)
    {
        bool hasChanged = _singleBlockListProcessor.ProcessToEditorValue(toEditorValue);

        if (toEditorValue is BlockListValue blockListValue
            && _blockListConfigurationCache.IsPropertyEditorBlockListConfiguredAsSingle(property.PropertyType.DataTypeKey))
        {
            value = _singleBlockListProcessor.ConvertBlockListToSingleBlock(blockListValue);
            return true;
        }

        value = toEditorValue;
        return hasChanged;
    }

    private class UpdateItem
    {
        public UpdateItem(PropertyDataDto propertyDataDto, IPropertyType propertyType, object? updatedValue)
        {
            PropertyDataDto = propertyDataDto;
            PropertyType = propertyType;
            UpdatedValue = updatedValue;
        }

        public object? UpdatedValue { get; set; }

        public PropertyDataDto PropertyDataDto { get; set; }
        public IPropertyType PropertyType { get; set; }
    }
}

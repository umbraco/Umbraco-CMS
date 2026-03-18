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

/// <summary>
/// Handles migration of single block list data structures during the upgrade to Umbraco version 18.0.0.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateSingleBlockList"/> class, responsible for migrating single block list data during the upgrade to version 18.0.0.
    /// </summary>
    /// <param name="context">The migration context providing information and services for the migration process.</param>
    /// <param name="umbracoContextFactory">Factory for creating Umbraco context instances.</param>
    /// <param name="languageService">Service for managing languages in Umbraco.</param>
    /// <param name="contentTypeService">Service for managing content types.</param>
    /// <param name="mediaTypeService">Service for managing media types.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="logger">The logger used for logging migration operations.</param>
    /// <param name="coreScopeProvider">Provides scope management for database operations.</param>
    /// <param name="singleBlockListProcessor">Processor for handling single block list migration logic.</param>
    /// <param name="jsonSerializer">Serializer for handling JSON data during migration.</param>
    /// <param name="blockListConfigurationCache">Cache for block list configuration data.</param>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors.</param>
    /// <param name="ioHelper">Helper for IO operations, such as file and path management.</param>
    /// <param name="blockValuePropertyIndexValueFactory">Factory for creating property index values for block values.</param>
    /// <param name="elementTypeCache">Cache for block editor element types.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
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
        // gets filled by all registered ITypedSingleBlockListProcessor
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

        // populate the cache to limit amount of db locks in recursion logic.
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

        // we want to batch actual update calls to the database, so we are grouping them by propertyEditorAlias
        // and again by propertyType(dataType).
        var updateItemsByPropertyEditorAlias = new Dictionary<string, Dictionary<IPropertyType, List<UpdateItem>>>();

        // For each propertyEditor, collect and process all propertyTypes and their propertyData
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

        // now that we have updated the configuration of all propertyTypes, we can save the updated propertyTypes
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
            // make sure the passed in data is valid and can be processed
            IDataType dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey)
                                 ?? throw new InvalidOperationException("The data type could not be fetched.");
            IDataValueEditor valueEditor = dataType.Editor?.GetValueEditor()
                                           ?? throw new InvalidOperationException(
                                               "The data type value editor could not be obtained.");

            // fetch all the propertyData for the current propertyType
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

            // process all the propertyData
            // if none of the processors modify the value, the propertyData is skipped from being saved.
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
            // The dataType and valueEditor should be constructed as we have done this before, but we hate null values.
            IDataType dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey)
                                 ?? throw new InvalidOperationException("The data type could not be fetched.");
            IDataValueEditor updatedValueEditor = dataType.Editor?.GetValueEditor()
                                           ?? throw new InvalidOperationException(
                                               "The data type value editor could not be obtained.");

            // batch by datatype
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

        // create a fake property to be able to get a typed value and run it trough the processors.
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

    /// <summary>
    /// Takes the updated value that was instanced from the db value by the old ValueEditors
    /// And runs it through the updated ValueEditors and sets it on the PropertyDataDto
    /// </summary>
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

    /// <summary>
    /// If the value is a BlockListValue, and its datatype is configured as single
    /// We also need to convert the outer BlockListValue to a SingleBlockValue
    /// Either way, we need to run the value through the processors to possibly update nested values
    /// </summary>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateItem"/> class with the specified property data, property type, and updated value.
        /// </summary>
        /// <param name="propertyDataDto">The <see cref="PropertyDataDto"/> representing the data for the property to be updated.</param>
        /// <param name="propertyType">The <see cref="IPropertyType"/> that defines the type of the property being updated.</param>
        /// <param name="updatedValue">The new value to assign to the property.</param>
        public UpdateItem(PropertyDataDto propertyDataDto, IPropertyType propertyType, object? updatedValue)
        {
            PropertyDataDto = propertyDataDto;
            PropertyType = propertyType;
            UpdatedValue = updatedValue;
        }

        /// <summary>
        /// Gets or sets the value that has been updated for this item during the migration process.
        /// This typically represents the new value assigned after migration logic is applied.
        /// </summary>
        public object? UpdatedValue { get; set; }

        /// <summary>
        /// Gets or sets the property data transfer object (DTO) associated with this update item.
        /// This object contains the data for a specific property being migrated in the single block list upgrade process.
        /// </summary>
        public PropertyDataDto PropertyDataDto { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IPropertyType"/> that is associated with this update item.
        /// </summary>
        public IPropertyType PropertyType { get; set; }
    }
}

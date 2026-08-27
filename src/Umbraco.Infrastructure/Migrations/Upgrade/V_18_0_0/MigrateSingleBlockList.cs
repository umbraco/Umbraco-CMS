using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private const int DefaultPageSize = 1000;

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
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public MigrateSingleBlockList(
        IMigrationContext context,
        IUmbracoContextFactory umbracoContextFactory,
        ILanguageService languageService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        ILogger<MigrateSingleBlockList> logger,
        ICoreScopeProvider coreScopeProvider,
        SingleBlockListProcessor singleBlockListProcessor,
        IJsonSerializer jsonSerializer,
        SingleBlockListConfigurationCache blockListConfigurationCache,
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory,
        IBlockEditorElementTypeCache elementTypeCache,
        AppCaches appCaches)
        : this(
            context,
            umbracoContextFactory,
            languageService,
            contentTypeService,
            mediaTypeService,
            dataTypeService,
            logger,
            coreScopeProvider,
            singleBlockListProcessor,
            jsonSerializer,
            blockListConfigurationCache,
            dataValueEditorFactory,
            ioHelper,
            blockValuePropertyIndexValueFactory,
            elementTypeCache,
            appCaches,
            StaticServiceProvider.Instance.GetRequiredService<IDataTypeConfigurationCache>())
    {
    }

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
    /// <param name="dataTypeConfigurationCache">Cache for data type configurations.</param>
    public MigrateSingleBlockList(
        IMigrationContext context,
        IUmbracoContextFactory umbracoContextFactory,
        ILanguageService languageService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        ILogger<MigrateSingleBlockList> logger,
        ICoreScopeProvider coreScopeProvider,
        SingleBlockListProcessor singleBlockListProcessor,
        IJsonSerializer jsonSerializer,
        SingleBlockListConfigurationCache blockListConfigurationCache,
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory,
        IBlockEditorElementTypeCache elementTypeCache,
        AppCaches appCaches,
        IDataTypeConfigurationCache dataTypeConfigurationCache)
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
        _dataTypeConfigurationCache = dataTypeConfigurationCache;

        _dummySingleBlockValueEditor = new SingleBlockPropertyEditor(dataValueEditorFactory, jsonSerializer, ioHelper, blockValuePropertyIndexValueFactory).GetValueEditor();
    }

    /// <summary>
    /// Gets the number of property data rows fetched, converted and saved at a time.
    /// </summary>
    /// <remarks>
    /// Deliberately lower than the page size other property data migrations use: this migration deserializes a
    /// whole block object graph per row, which is several times the size of the stored JSON it came from.
    /// Overridable so tests can exercise the paging loop without creating thousands of rows.
    /// </remarks>
    internal virtual int PageSize => DefaultPageSize;

    protected override async Task MigrateAsync()
    {
        // Give scope for the migration to complete within the command timeout, which may be necessary on large datasets.
        EnsureLongCommandTimeout(Database);

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

        IDataType[] singleBlockListDataTypes = _blockListConfigurationCache.CachedDataTypes.ToArray();
        var singleBlockListDataTypeKeys = singleBlockListDataTypes.Select(dataType => dataType.Key).ToHashSet();

        // Save the converted property values first, and only switch the data types over below.
        //
        // This ordering is load-bearing, not cosmetic. The value editors that re-serialize a converted value resolve
        // the value editor of each nested block property from its data type's property editor alias, and they do so on
        // their own scopes - and therefore their own connections - which cannot observe anything this migration has
        // written but not committed. Converting first means those lookups only ever read committed, pre-migration
        // state, and SingleBlockMigrationEditorAliasOverride is what routes the converted values to the single block
        // value editor regardless (https://github.com/umbraco/Umbraco-CMS/issues/23596).
        //
        // Each page of property data is converted and saved before the next one is fetched, so that neither the
        // fetched rows nor the values they deserialize to accumulate across the whole site
        // (https://github.com/umbraco/Umbraco-CMS/issues/23766). That does not weaken the ordering above, which
        // constrains when umbracoDataType is written, not umbracoPropertyData.
        foreach (var propertyEditorAlias in propertyEditorAliases)
        {
            if (relevantPropertyEditors.TryGetValue(propertyEditorAlias, out IPropertyType[]? propertyTypes) is false)
            {
                continue;
            }

            _logger.LogInformation(
                "Migration starting for all properties of type: {propertyEditorAlias}",
                propertyEditorAlias);

            var success = true;
            var foundPropertyData = false;

            foreach (IPropertyType propertyType in propertyTypes)
            {
                (bool hadPropertyData, bool propertyTypeSucceeded) =
                    await MigratePropertyTypeAsync(propertyType, languagesById, singleBlockListDataTypeKeys);

                foundPropertyData |= hadPropertyData;
                success &= propertyTypeSucceeded;
            }

            // Reported when no property type of this editor alias had any candidate property data at all - not
            // when none of it needed converting, which is a normal outcome.
            if (foundPropertyData is false)
            {
                _logger.LogInformation(
                    "No properties have been found to migrate for {propertyEditorAlias}",
                    propertyEditorAlias);
                continue;
            }

            if (success)
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

        // update the configuration of all propertyTypes
        var singleBlockListDataTypesIds = singleBlockListDataTypes.Select(type => type.Id).ToList();

        string updateSql = $@"
UPDATE umbracoDataType
SET propertyEditorAlias = '{Constants.PropertyEditors.Aliases.SingleBlock}',
    propertyEditorUiAlias = 'Umb.PropertyEditorUi.BlockSingle'
WHERE nodeId IN (@0)";
        await Database.ExecuteAsync(updateSql, singleBlockListDataTypesIds);

        // the element type cache, and the isolated/runtime caches it is built from in the default implementation,
        // still describe the data types as they were before the update - as does the data type configuration cache,
        // which is backed by its own memory cache rather than the application caches
        _elementTypeCache.ClearAll();
        _appCaches.IsolatedCaches.ClearAllCaches();
        _appCaches.RuntimeCache.Clear();
        _dataTypeConfigurationCache.ClearCache(singleBlockListDataTypeKeys);
        RebuildCache = true;
    }

    /// <summary>
    /// Converts and saves the property data of a single property type, a page at a time.
    /// </summary>
    /// <returns>
    /// Whether the property type had any candidate property data at all, and whether every value that needed
    /// converting could be converted.
    /// </returns>
    private async Task<(bool HadPropertyData, bool Success)> MigratePropertyTypeAsync(
        IPropertyType propertyType,
        IDictionary<int, ILanguage> languagesById,
        IReadOnlySet<Guid> singleBlockListDataTypeKeys)
    {
        // make sure the passed in data is valid and can be processed
        IDataType dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey)
                             ?? throw new InvalidOperationException("The data type could not be fetched.");
        IDataValueEditor valueEditor = dataType.Editor?.GetValueEditor()
                                       ?? throw new InvalidOperationException(
                                           "The data type value editor could not be obtained.");

        var total = await Database.ExecuteScalarAsync<long>(BuildPropertyDataCountSql(propertyType));
        if (total == 0)
        {
            return (false, true);
        }

        _logger.LogInformation(
            "Migrating {PropertyDataCount} property data values for property {PropertyTypeAlias} ({PropertyTypeKey}) with property editor alias {PropertyEditorAlias}",
            total,
            propertyType.Alias,
            propertyType.Key,
            propertyType.PropertyEditorAlias);

        var pageSize = PageSize;
        var progress = new MigrationProgress(total);
        var success = true;
        var converted = 0;

        // Keyset paging, restarting from zero for each property type: the page is the next rows by id rather than
        // an offset into the result set, so every page costs the same and no row can be visited twice or skipped.
        // That holds because nothing this migration writes touches a column the query filters or orders on - the
        // batched update below only ever writes a non-empty textValue.
        var lastId = 0;

        while (true)
        {
            List<PropertyDataDto> page = await Database.FetchAsync<PropertyDataDto>(
                BuildPropertyDataPageSql(propertyType, lastId, pageSize));

            if (page.Count == 0)
            {
                break;
            }

            lastId = page[^1].Id;

            (bool pageSucceeded, int pageConverted) = ConvertAndSavePage(
                page, propertyType, languagesById, valueEditor, singleBlockListDataTypeKeys, progress);

            success &= pageSucceeded;
            converted += pageConverted;

            if (page.Count < pageSize)
            {
                break;
            }
        }

        if (converted > 0)
        {
            _logger.LogDebug(
                "Migration completed for property type: {propertyTypeName} (id: {propertyTypeId}, key: {propertyTypeKey}, alias: {propertyTypeAlias}, editor alias: {propertyTypeEditorAlias}) - {updateCount} property DTO entries updated.",
                propertyType.Name,
                propertyType.Id,
                propertyType.Key,
                propertyType.Alias,
                propertyType.PropertyEditorAlias,
                converted);
        }

        return (true, success);
    }

    /// <summary>
    /// Counts the property data of a property type that is a candidate for conversion, for progress reporting. No
    /// ordering, as an ordered count would only add an avoidable sort.
    /// </summary>
    private Sql<ISqlContext> BuildPropertyDataCountSql(IPropertyType propertyType)
        => AddPropertyDataFilter(Sql().SelectCount(), propertyType);

    /// <summary>
    /// Selects the next page of a property type's candidate property data, by keyset rather than by offset.
    /// </summary>
    private Sql<ISqlContext> BuildPropertyDataPageSql(IPropertyType propertyType, int lastId, int pageSize)
        => AddPropertyDataFilter(Sql().Select<PropertyDataDto>(), propertyType)
            .Where<PropertyDataDto>(propertyData => propertyData.Id > lastId)
            .OrderBy<PropertyDataDto>(propertyData => propertyData.Id)

            // Applied last: SQL Server inserts "TOP n" after SELECT, but SQLite appends "LIMIT n" to the statement.
            .SelectTop(pageSize);

    private static Sql<ISqlContext> AddPropertyDataFilter(Sql<ISqlContext> sql, IPropertyType propertyType)
        => sql.From<PropertyDataDto>()
            .InnerJoin<ContentVersionDto>()
            .On<PropertyDataDto, ContentVersionDto>((propertyData, contentVersion) =>
                propertyData.VersionId == contentVersion.Id)
            .LeftJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>((contentVersion, documentVersion) =>
                contentVersion.Id == documentVersion.Id)
            .Where<PropertyDataDto, ContentVersionDto, DocumentVersionDto>((propertyData, contentVersion, documentVersion) =>
                (contentVersion.Current == true || documentVersion.Published == true)
                && propertyData.PropertyTypeId == propertyType.Id

                // Block and rich text values are held as text, but PropertyDataDto.Value falls back to the varchar
                // column before the text one, so a row only has nothing to convert when both are empty.
                && (propertyData.TextValue != null || propertyData.VarcharValue != null));

    /// <summary>
    /// Converts a page of property data and persists whatever converted, leaving the rest of the rows untouched.
    /// </summary>
    private (bool Success, int Converted) ConvertAndSavePage(
        List<PropertyDataDto> page,
        IPropertyType propertyType,
        IDictionary<int, ILanguage> languagesById,
        IDataValueEditor valueEditor,
        IReadOnlySet<Guid> singleBlockListDataTypeKeys,
        MigrationProgress progress)
    {
        // The snapshot is taken before the value is converted, so the batched update only writes the columns that
        // actually changed. Database belongs to the ambient scope and is not thread safe, so it is only touched
        // here, never from the workers below.
        var updateBatch = page
            .Select(propertyDataDto => UpdateBatch.For(propertyDataDto, Database.StartSnapshot(propertyDataDto)))
            .ToList();

        // Keyed by property data id, which is unique within a page, so a worker's outcome can be looked up
        // directly rather than by scanning the batch.
        var results = new ConcurrentDictionary<int, ConversionResult>();

        void HandleUpdateBatch(UpdateBatch<PropertyDataDto> update)
        {
            using UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

            var completed = progress.Increment();
            if (completed % 100 == 0)
            {
                _logger.LogInformation("  - finished {Progress} of {Total} properties", completed, progress.Total);
            }

            results[update.Poco.Id] = ConvertPropertyDataDto(
                update.Poco, propertyType, languagesById, valueEditor, singleBlockListDataTypeKeys);
        }

        RunUpdateBatch(updateBatch, HandleUpdateBatch);

        var refused = 0;
        updateBatch.RemoveAll(update =>
        {
            ConversionResult result = results[update.Poco.Id];
            if (result is ConversionResult.Refused)
            {
                refused++;
            }

            return result is not ConversionResult.Converted;
        });

        if (updateBatch.Count == 0)
        {
            _logger.LogDebug("  - no properties to convert, continuing");
            return (refused == 0, 0);
        }

        _logger.LogInformation("  - {totalConverted} properties converted, saving...", updateBatch.Count);
        var result = Database.UpdateBatch(updateBatch, new BatchOptions { BatchSize = 100 });
        if (result != updateBatch.Count)
        {
            throw new InvalidOperationException(
                $"The database batch update was supposed to update {updateBatch.Count} property DTO entries, but it updated {result} entries.");
        }

        return (refused == 0, updateBatch.Count);
    }

    private void RunUpdateBatch(
        List<UpdateBatch<PropertyDataDto>> updateBatch,
        Action<UpdateBatch<PropertyDataDto>> handleUpdateBatch)
    {
        if (DatabaseType == DatabaseType.SQLite)
        {
            // SQLite locks up if we run the migration in parallel, so... let's not.
            foreach (UpdateBatch<PropertyDataDto> update in updateBatch)
            {
                handleUpdateBatch(update);
            }

            return;
        }

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
                        handleUpdateBatch(update);
                    },
                    token);
            }

            await task;
        }).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Converts a single property data value, setting the converted value on the DTO ready to be persisted.
    /// </summary>
    private ConversionResult ConvertPropertyDataDto(
        PropertyDataDto propertyDataDto,
        IPropertyType propertyType,
        IDictionary<int, ILanguage> languagesById,
        IDataValueEditor valueEditor,
        IReadOnlySet<Guid> singleBlockListDataTypeKeys)
    {
        var cultureResult = PropertyDataCultureResolver.ResolveCulture(propertyType, propertyDataDto.LanguageId, languagesById);
        if (cultureResult.ShouldSkip)
        {
            _logger.LogWarning(
                PropertyDataCultureResolver.OrphanedLanguageWarningTemplate,
                propertyDataDto.Id,
                cultureResult.OrphanedLanguageId,
                propertyType.Name,
                propertyType.Id,
                propertyType.Key,
                propertyType.Alias);
            return ConversionResult.Skipped;
        }

        var culture = cultureResult.Culture;

        // create a fake property to be able to get a typed value and run it through the processors.
        var segment = propertyType.VariesBySegment() ? propertyDataDto.Segment : null;
        var property = PropertyDataCultureResolver.CreateMigrationProperty(propertyType, propertyDataDto.Value, culture, segment);

        // No editor alias override around this: the value is read as it is still stored, which is exactly what the
        // property type's own value editor is for.
        var toEditorValue = valueEditor.ToEditor(property, culture, segment);

        if (TryTransformValue(toEditorValue, property, out var updatedValue) is false)
        {
            _logger.LogDebug(
                "    - skipping as no processor modified the data for property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, key: {propertyTypeKey}, alias: {propertyTypeAlias})",
                propertyDataDto.Id,
                propertyType.Name,
                propertyType.Id,
                propertyType.Key,
                propertyType.Alias);
            return ConversionResult.Skipped;
        }

        // The override only affects re-serialization, and it has to be applied per value rather than around the
        // loop: the parallelized path deliberately does not flow the execution context, which is what an ambient
        // AsyncLocal rides on.
        using (SingleBlockMigrationEditorAliasOverride.For(singleBlockListDataTypeKeys))
        {
            return FinalizeUpdateItem(new UpdateItem(propertyDataDto, propertyType, updatedValue), valueEditor)
                ? ConversionResult.Converted
                : ConversionResult.Refused;
        }
    }

    /// <summary>
    /// Serializes the converted value back to its database representation and sets it on the PropertyDataDto.
    /// </summary>
    private bool FinalizeUpdateItem(UpdateItem updateItem, IDataValueEditor valueEditor)
    {
        var editorValue = _jsonSerializer.Serialize(updateItem.UpdatedValue);

        // Re-running FromEditor here is only to re-serialize the converted value; the referenced-entity
        // caching it would otherwise trigger is wasted work that issues per-property content/media reads
        // in separate scopes, contending with this migration's own scope. Suppress it.
        object? dbValue;
#pragma warning disable CS0618 // Type or member is obsolete
        using (CacheReferencedEntitiesSuppression.Suppress())
        {
            dbValue = updateItem.UpdatedValue is SingleBlockValue
                ? _dummySingleBlockValueEditor.FromEditor(new ContentPropertyData(editorValue, null), null)
                : valueEditor.FromEditor(new ContentPropertyData(editorValue, null), null);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (dbValue is not string stringValue || stringValue.DetectIsJson() is false)
        {
            // Anything but a JSON string would replace the stored value, so the row is left untouched. Losing a
            // value that held content is an error; an empty one converting to nothing is expected.
            LogFailedConversion(
                updateItem,
                "the value editor did not yield a valid JSON string as its FromEditor value");
            return false;
        }

        // The conversions happen on the in-memory value, but the value that gets persisted is produced by the
        // containing value editor, which resolves the value editor of each nested block property itself. If it
        // resolves the wrong one the nested value is replaced with null while the outer value stays valid JSON, so
        // the conversions are counted on both sides to make that loss detectable.
        var expectedSingleBlockCount = SingleBlockConversionVerifier.CountSingleBlockValues(updateItem.UpdatedValue);
        var actualSingleBlockCount = SingleBlockConversionVerifier.CountSingleBlockLayouts(stringValue);
        if (actualSingleBlockCount < expectedSingleBlockCount)
        {
            LogFailedConversion(
                updateItem,
                $"only {actualSingleBlockCount} of {expectedSingleBlockCount} converted single block values survived being serialized for persistence");
            return false;
        }

        updateItem.PropertyDataDto.TextValue = stringValue;
        return true;
    }

    private void LogFailedConversion(UpdateItem updateItem, string reason)
    {
        const string MessageTemplate =
            "    - refused to update property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, key: {propertyTypeKey}, alias: {propertyTypeAlias}) as {reason}. The stored value is left as it was.";

        if (updateItem.PropertyDataDto.TextValue.IsNullOrWhiteSpace())
        {
            _logger.LogWarning(
                MessageTemplate,
                updateItem.PropertyDataDto.Id,
                updateItem.PropertyType.Name,
                updateItem.PropertyType.Id,
                updateItem.PropertyType.Key,
                updateItem.PropertyType.Alias,
                reason);
            return;
        }

        _logger.LogError(
            MessageTemplate,
            updateItem.PropertyDataDto.Id,
            updateItem.PropertyType.Name,
            updateItem.PropertyType.Id,
            updateItem.PropertyType.Key,
            updateItem.PropertyType.Alias,
            reason);
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

            // the conversion returns the value unchanged when there is no block to convert
            return hasChanged || ReferenceEquals(value, blockListValue) is false;
        }

        value = toEditorValue;
        return hasChanged;
    }

    private enum ConversionResult
    {
        /// <summary>There was nothing to convert. The row is left untouched and the migration still succeeds.</summary>
        Skipped,

        /// <summary>The converted value has been set on the DTO and is ready to be persisted.</summary>
        Converted,

        /// <summary>
        /// The value could not be converted safely. The row is left untouched and the migration of its property
        /// editor alias is reported as failed.
        /// </summary>
        Refused,
    }

    /// <summary>
    /// Tracks how far through a property type's property data the migration has got, across all of its pages.
    /// </summary>
    private sealed class MigrationProgress
    {
        private long _processed;

        public MigrationProgress(long total) => Total = total;

        public long Total { get; }

        /// <summary>
        /// Counts one more processed property data value and returns the running total. Safe to call from the
        /// parallelized conversion workers.
        /// </summary>
        public long Increment() => Interlocked.Increment(ref _processed);
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

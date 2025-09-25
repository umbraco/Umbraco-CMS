using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
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
    private readonly ILogger<MigrateSingleBlockList> _logger;

    public MigrateSingleBlockList(
        IMigrationContext context,
        IUmbracoContextFactory umbracoContextFactory,
        ILanguageService languageService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        ILogger<MigrateSingleBlockList> logger,
        ICoreScopeProvider coreScopeProvider,
        SingleBlockListProcessor  singleBlockListProcessor)
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
    }

    protected override async Task MigrateAsync()
    {
        using UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();
        var languagesById = (await _languageService.GetAllAsync())
            .ToDictionary(language => language.Id);

        IEnumerable<IContentType> allContentTypes = _contentTypeService.GetAll();
        IEnumerable<IPropertyType> contentPropertyTypes = allContentTypes
            .SelectMany(ct => ct.PropertyTypes);

        IMediaType[] allMediaTypes = _mediaTypeService.GetAll().ToArray();
        IEnumerable<IPropertyType> mediaPropertyTypes = allMediaTypes
            .SelectMany(ct => ct.PropertyTypes);

        // get all blockListPropertyTypes
        IEnumerable<IPropertyType> blockListPropertyTypes =
            contentPropertyTypes.Concat(mediaPropertyTypes).DistinctBy(pt => pt.Id)
                .Where(pt => pt.PropertyEditorAlias == Constants.PropertyEditors.Aliases.BlockList);

        // update all propertyData for each propertyType
        foreach (IPropertyType propertyType in blockListPropertyTypes)
        {
            await ProcessPropertyType(propertyType, languagesById);
        }

        // update the configuration of all propertyTypes
    }

    private async Task ProcessPropertyType(IPropertyType propertyType, IDictionary<int, ILanguage> languagesById)
    {
        IDataType dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey)
                             ?? throw new InvalidOperationException("The data type could not be fetched.");

        BlockListConfiguration configuration = dataType.ConfigurationAs<BlockListConfiguration>()
                                               ?? throw new InvalidOperationException(
                                                   "The data type configuration could not be obtained.");

        if (configuration.UseSingleBlockMode is false)
        {
            return;
        }

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
            return;
        }

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
                _logger.LogInformation("  - finíshed {progress} of {total} properties", progress,
                    updateBatch.Count);
            }

            PropertyDataDto propertyDataDto = update.Poco;

            if (ProcessPropertyDataDto(propertyDataDto, propertyType, languagesById, valueEditor) is false)
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
            return;
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

    private bool ProcessPropertyDataDto(PropertyDataDto propertyDataDto, IPropertyType propertyType,
        IDictionary<int, ILanguage> languagesById, IDataValueEditor valueEditor)
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
            return false;
        }

        var segment = propertyType.VariesBySegment() ? propertyDataDto.Segment : null;
        var property = new Property(propertyType);
        property.SetValue(propertyDataDto.Value, culture, segment);
        var toEditorValue = valueEditor.ToEditor(property, culture, segment);

        if (_singleBlockListProcessor.ProcessToEditorValue(toEditorValue) == false)
        {
            _logger.LogDebug(
                "    - skipping as no processor modified the data for property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                propertyDataDto.Id,
                propertyType.Name,
                propertyType.Id,
                propertyType.Alias);
            return false;
        }

        // var editorValue = _jsonSerializer.Serialize(toEditorValue);
        // var dbValue = valueEditor.FromEditor(new ContentPropertyData(editorValue, null), null);
        // if (dbValue is not string stringValue || stringValue.DetectIsJson() is false)
        // {
        //     _logger.LogWarning(
        //         "    - value editor did not yield a valid JSON string as FromEditor value property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
        //         propertyDataDto.Id,
        //         propertyType.Name,
        //         propertyType.Id,
        //         propertyType.Alias);
        //     return false;
        // }
        //
        // propertyDataDto.TextValue = stringValue;
        return true;
    }
}

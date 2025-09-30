using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_1_0;

public class FixConvertLocalLinks : MigrationBase
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<FixConvertLocalLinks> _logger;
    private readonly IDataTypeService _dataTypeService;
    private readonly ILanguageService _languageService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly LocalLinkProcessorForFaultyLinks _localLinkProcessor;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly LocalLinkMigrationTracker _localLinkMigrationTracker;

    public FixConvertLocalLinks(
        IMigrationContext context,
        IUmbracoContextFactory umbracoContextFactory,
        IContentTypeService contentTypeService,
        ILogger<FixConvertLocalLinks> logger,
        IDataTypeService dataTypeService,
        ILanguageService languageService,
        IJsonSerializer jsonSerializer,
        LocalLinkProcessorForFaultyLinks localLinkProcessor,
        IMediaTypeService mediaTypeService,
        ICoreScopeProvider coreScopeProvider,
        LocalLinkMigrationTracker localLinkMigrationTracker)
        : base(context)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _contentTypeService = contentTypeService;
        _logger = logger;
        _dataTypeService = dataTypeService;
        _languageService = languageService;
        _jsonSerializer = jsonSerializer;
        _localLinkProcessor = localLinkProcessor;
        _mediaTypeService = mediaTypeService;
        _coreScopeProvider = coreScopeProvider;
        _localLinkMigrationTracker = localLinkMigrationTracker;
    }

    protected override void Migrate()
    {
        // the original migration was fixed, we only run this if
        // this migration hits
        // and the fixed original migration has not run, so it must have run in the past
        if (_localLinkMigrationTracker.HasFixedMigrationRun)
        {
            return;
        }

        IEnumerable<string> propertyEditorAliases = _localLinkProcessor.GetSupportedPropertyEditorAliases();

        using UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();
        var languagesById = _languageService.GetAllAsync().GetAwaiter().GetResult()
            .ToDictionary(language => language.Id);

        IEnumerable<IContentType> allContentTypes = _contentTypeService.GetAll();
        IEnumerable<IPropertyType> contentPropertyTypes = allContentTypes
            .SelectMany(ct => ct.PropertyTypes);

        IMediaType[] allMediaTypes = _mediaTypeService.GetAll().ToArray();
        IEnumerable<IPropertyType> mediaPropertyTypes = allMediaTypes
            .SelectMany(ct => ct.PropertyTypes);

        var relevantPropertyEditors =
            contentPropertyTypes.Concat(mediaPropertyTypes).DistinctBy(pt => pt.Id)
                .Where(pt => propertyEditorAliases.Contains(pt.PropertyEditorAlias))
                .GroupBy(pt => pt.PropertyEditorAlias)
                .ToDictionary(group => group.Key, group => group.ToArray());


        foreach (var propertyEditorAlias in propertyEditorAliases)
        {
            if (relevantPropertyEditors.TryGetValue(propertyEditorAlias, out IPropertyType[]? propertyTypes) is false)
            {
                continue;
            }

            _logger.LogInformation(
                "Migration starting for all properties of type: {propertyEditorAlias}",
                propertyEditorAlias);
            if (ProcessPropertyTypes(propertyTypes, languagesById))
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

        RebuildCache = true;
    }

    private bool ProcessPropertyTypes(IPropertyType[] propertyTypes, IDictionary<int, ILanguage> languagesById)
    {
        foreach (IPropertyType propertyType in propertyTypes)
        {
            IDataType dataType = _dataTypeService.GetAsync(propertyType.DataTypeKey).GetAwaiter().GetResult()
                                 ?? throw new InvalidOperationException("The data type could not be fetched.");

            IDataValueEditor valueEditor = dataType.Editor?.GetValueEditor()
                                           ?? throw new InvalidOperationException(
                                               "The data type value editor could not be fetched.");

            Sql<ISqlContext> sql = Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .InnerJoin<ContentVersionDto>()
                .On<PropertyDataDto, ContentVersionDto>((propertyData, contentVersion) =>
                    propertyData.VersionId == contentVersion.Id)
                .LeftJoin<DocumentVersionDto>()
                .On<ContentVersionDto, DocumentVersionDto>((contentVersion, documentVersion) =>
                    contentVersion.Id == documentVersion.Id)
                .Where<PropertyDataDto, ContentVersionDto, DocumentVersionDto>(
                    (propertyData, contentVersion, documentVersion) =>
                        (contentVersion.Current == true || documentVersion.Published == true)
                        && propertyData.PropertyTypeId == propertyType.Id);

            List<PropertyDataDto> propertyDataDtos = Database.Fetch<PropertyDataDto>(sql);
            if (propertyDataDtos.Count < 1)
            {
                continue;
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

                if (ProcessPropertyDataDto(propertyDataDto, propertyType, languagesById, valueEditor) == false)
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

        if (_localLinkProcessor.ProcessToEditorValue(toEditorValue) == false)
        {
            _logger.LogDebug(
                "    - skipping as no processor modified the data for property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                propertyDataDto.Id,
                propertyType.Name,
                propertyType.Id,
                propertyType.Alias);
            return false;
        }

        var editorValue = _jsonSerializer.Serialize(toEditorValue);
        var dbValue = valueEditor.FromEditor(new ContentPropertyData(editorValue, null), null);
        if (dbValue is not string stringValue || stringValue.DetectIsJson() is false)
        {
            _logger.LogWarning(
                "    - value editor did not yield a valid JSON string as FromEditor value property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                propertyDataDto.Id,
                propertyType.Name,
                propertyType.Id,
                propertyType.Alias);
            return false;
        }

        propertyDataDto.TextValue = stringValue;
        return true;
    }
}

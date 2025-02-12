using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Will be removed in V18")]
public abstract class ConvertBlockEditorPropertiesBase : MigrationBase
{
    private readonly ILogger<ConvertBlockEditorPropertiesBase> _logger;
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly ILanguageService _languageService;
    private readonly ICoreScopeProvider _coreScopeProvider;

    protected abstract IEnumerable<string> PropertyEditorAliases { get; }

    protected abstract EditorValueHandling DetermineEditorValueHandling(object editorValue);

    protected bool SkipMigration { get; init; }

    protected bool ParallelizeMigration { get; init; }

    protected enum EditorValueHandling
    {
        IgnoreConversion,
        ProceedConversion,
        HandleAsError
    }

    public ConvertBlockEditorPropertiesBase(
        IMigrationContext context,
        ILogger<ConvertBlockEditorPropertiesBase> logger,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IJsonSerializer jsonSerializer,
        IUmbracoContextFactory umbracoContextFactory,
        ILanguageService languageService,
        ICoreScopeProvider coreScopeProvider)
        : base(context)
    {
        _logger = logger;
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
        _jsonSerializer = jsonSerializer;
        _umbracoContextFactory = umbracoContextFactory;
        _languageService = languageService;
        _coreScopeProvider = coreScopeProvider;
    }

    protected override void Migrate()
    {
        if (SkipMigration)
        {
            _logger.LogInformation("Migration was skipped due to configuration.");
            return;
        }

        using UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();
        var languagesById = _languageService.GetAllAsync().GetAwaiter().GetResult()
            .ToDictionary(language => language.Id);
        IContentType[] allContentTypes = _contentTypeService.GetAll().ToArray();
        var allPropertyTypesByEditor = allContentTypes
            .SelectMany(ct => ct.PropertyTypes)
            .GroupBy(pt => pt.PropertyEditorAlias)
            .ToDictionary(group => group.Key, group => group.ToArray());


        foreach (var propertyEditorAlias in PropertyEditorAliases)
        {
            if (allPropertyTypesByEditor.TryGetValue(propertyEditorAlias, out IPropertyType[]? propertyTypes) is false)
            {
                continue;
            }

            _logger.LogInformation(
                "Migration starting for all properties of type: {propertyEditorAlias}",
                propertyEditorAlias);
            if (Handle(propertyTypes, languagesById))
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

    protected virtual object UpdateEditorValue(object editorValue) => editorValue;

    protected virtual string UpdateDatabaseValue(string dbValue) => dbValue;

    private bool Handle(IPropertyType[] propertyTypes, IDictionary<int, ILanguage> languagesById)
    {
        var success = true;

        var propertyTypeCount = propertyTypes.Length;
        for (var propertyTypeIndex = 0; propertyTypeIndex < propertyTypeCount; propertyTypeIndex++)
        {
            IPropertyType propertyType = propertyTypes[propertyTypeIndex];
            try
            {
                _logger.LogInformation(
                    "- starting property type {propertyTypeIndex}/{propertyTypeCount} : {propertyTypeName} (id: {propertyTypeId}, alias: {propertyTypeAlias})...",
                    propertyTypeIndex + 1,
                    propertyTypeCount,
                    propertyType.Name, propertyType.Id, propertyType.Alias);
                IDataType dataType = _dataTypeService.GetAsync(propertyType.DataTypeKey).GetAwaiter().GetResult()
                                     ?? throw new InvalidOperationException("The data type could not be fetched.");

                if (IsCandidateForMigration(propertyType, dataType) is false)
                {
                    _logger.LogInformation("  - skipped property type migration because it was not a applicable.");
                    continue;
                }

                IDataValueEditor valueEditor = dataType.Editor?.GetValueEditor()
                                               ?? throw new InvalidOperationException(
                                                   "The data type value editor could not be fetched.");

                Sql<ISqlContext> sql = Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .Where<PropertyDataDto>(dto => dto.PropertyTypeId == propertyType.Id);
                List<PropertyDataDto> propertyDataDtos = Database.Fetch<PropertyDataDto>(sql);
                if (propertyDataDtos.Any() is false)
                {
                    continue;
                }

                var updateBatch = propertyDataDtos.Select(propertyDataDto =>
                    UpdateBatch.For(propertyDataDto, Database.StartSnapshot(propertyDataDto))).ToList();

                var updatesToSkip = new ConcurrentBag<UpdateBatch<PropertyDataDto>>();

                var progress = 0;

                void HandleUpdateBatch(UpdateBatch<PropertyDataDto> update)
                {

                            using UmbracoContextReference umbracoContextReference =
                                _umbracoContextFactory.EnsureUmbracoContext();

                            progress++;
                            if (progress % 100 == 0)
                            {
                        _logger.LogInformation("  - finíshed {progress} of {total} properties", progress, updateBatch.Count);
                            }

                            PropertyDataDto propertyDataDto = update.Poco;

                            // NOTE: some old property data DTOs can have variance defined, even if the property type no longer varies
                            var culture = propertyType.VariesByCulture()
                                          && propertyDataDto.LanguageId.HasValue
                                          && languagesById.TryGetValue(
                                              propertyDataDto.LanguageId.Value,
                                              out ILanguage? language)
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
                                return;
                            }

                            var segment = propertyType.VariesBySegment() ? propertyDataDto.Segment : null;
                            var property = new Property(propertyType);
                            property.SetValue(propertyDataDto.Value, culture, segment);
                            var toEditorValue = valueEditor.ToEditor(property, culture, segment);
                            switch (toEditorValue)
                            {
                                case null:
                                    _logger.LogWarning(
                                        "    - value editor yielded a null value for property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                                        propertyDataDto.Id,
                                        propertyType.Name,
                                        propertyType.Id,
                                        propertyType.Alias);
                                    updatesToSkip.Add(update);
                                    return;

                                case string str when str.IsNullOrWhiteSpace():
                                    // indicates either an empty block editor or corrupt block editor data - we can't do anything about either here
                                    updatesToSkip.Add(update);
                                    return;

                                default:
                                    switch (DetermineEditorValueHandling(toEditorValue))
                                    {
                                        case EditorValueHandling.IgnoreConversion:
                                            // nothing to convert, continue
                                            updatesToSkip.Add(update);
                                            return;
                                        case EditorValueHandling.ProceedConversion:
                                            // continue the conversion
                                            break;
                                        case EditorValueHandling.HandleAsError:
                                            _logger.LogError(
                                                "    - value editor did not yield a valid ToEditor value for property data with id: {propertyDataId} - the value type was {valueType} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                                                propertyDataDto.Id,
                                                toEditorValue.GetType(),
                                                propertyType.Name,
                                                propertyType.Id,
                                                propertyType.Alias);
                                            updatesToSkip.Add(update);
                                            return;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }

                                    break;
                            }

                            toEditorValue = UpdateEditorValue(toEditorValue);

                            var editorValue = _jsonSerializer.Serialize(toEditorValue);
                            var dbValue = valueEditor.FromEditor(new ContentPropertyData(editorValue, null), null);
                            if (dbValue is not string stringValue || stringValue.DetectIsJson() is false)
                            {
                                _logger.LogError(
                                    "    - value editor did not yield a valid JSON string as FromEditor value property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                                    propertyDataDto.Id,
                                    propertyType.Name,
                                    propertyType.Id,
                                    propertyType.Alias);
                                updatesToSkip.Add(update);
                                return;
                            }

                            stringValue = UpdateDatabaseValue(stringValue);

                            propertyDataDto.TextValue = stringValue;
                }

                if (ParallelizeMigration is false || DatabaseType == DatabaseType.SQLite)
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
                    _logger.LogInformation("  - no properties to convert, continuing");
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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Migration failed for property type: {propertyTypeName} (id: {propertyTypeId}, alias: {propertyTypeAlias}, editor alias: {propertyTypeEditorAlias})",
                    propertyType.Name,
                    propertyType.Id,
                    propertyType.Alias,
                    propertyType.PropertyEditorAlias);

                success = false;
            }
        }

        return success;
    }

    protected virtual bool IsCandidateForMigration(IPropertyType propertyType, IDataType dataType)
        => true;
}

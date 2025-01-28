using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Migrations.PreMigration;

// we use a composer here so its easier to clean this up when we no longer need it.
// same for additional classes in the same file, nice and self contained
// should only be used to migrate from v13 to v14
// ⚠️ FIXME: PLEASE DELETE THIS IN V14! ⚠️
public class DataTypeSplitDataCollectorComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, DataTypeSplitDataCollector>();
    }
}

public class DataTypeSplitDataCollector : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly DataEditorCollection _dataEditors;
    private readonly IManifestParser _manifestParser;
    private readonly IDataTypeService _dataTypeService;
    private readonly IKeyValueService _keyValueService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<DataTypeSplitDataCollector> _logger;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IUmbracoVersion _umbracoVersion;

    public DataTypeSplitDataCollector(
        DataEditorCollection dataEditors,
        IManifestParser manifestParser,
        IDataTypeService dataTypeService,
        IKeyValueService keyValueService,
        IJsonSerializer jsonSerializer,
        ILogger<DataTypeSplitDataCollector> logger,
        ICoreScopeProvider coreScopeProvider,
        IRuntimeState runtimeState,
        IServerRoleAccessor serverRoleAccessor,
        IUmbracoVersion umbracoVersion)
    {
        _dataEditors = dataEditors;
        _manifestParser = manifestParser;
        _dataTypeService = dataTypeService;
        _keyValueService = keyValueService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
        _coreScopeProvider = coreScopeProvider;
        _runtimeState = runtimeState;
        _serverRoleAccessor = serverRoleAccessor;
        _umbracoVersion = umbracoVersion;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        // should only be used to collect data in v13
        if (_umbracoVersion.Version.Major is not 13)
        {
            return;
        }

        // only run this if the application is actually running and not in an install/upgrade state
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        // do not run on load balanced subscribers
        if (_serverRoleAccessor.CurrentServerRole == ServerRole.Subscriber)
        {
            return;
        }

        var manifestEditorsData = _manifestParser.CombinedManifest.PropertyEditors
            .Select(pe => new EditorAliasSplitData(pe.Alias){EditorUiAlias = pe.Alias, EditorAlias = EditorAliasFromValueEditorValueType(pe.GetValueEditor().ValueType)})
            .ToDictionary(data => data.OriginalEditorAlias);

        _logger.LogDebug("Found {count} custom PropertyEditor(s) configured trough manifest files",manifestEditorsData.Count);

        var fromCodeEditorsData = _dataEditors
            .Where(de =>
                de.Type == EditorType.PropertyValue
                && de.GetType().Assembly.GetName().FullName
                    .StartsWith("umbraco.core", StringComparison.InvariantCultureIgnoreCase) is false
                && de.GetType().Assembly.GetName().FullName
                    .StartsWith("umbraco.infrastructure", StringComparison.InvariantCultureIgnoreCase) is false)
            .Select(de => new EditorAliasSplitData(de.Alias) { EditorAlias = de.Alias })
            .ToDictionary(data => data.OriginalEditorAlias);

        _logger.LogDebug("Found {count} custom PropertyEditor(s) configured trough code",fromCodeEditorsData.Count);

        var combinedEditorsData = new Dictionary<string, EditorAliasSplitData>(manifestEditorsData);
        foreach (KeyValuePair<string,EditorAliasSplitData> pair in fromCodeEditorsData)
        {
            combinedEditorsData.Add(pair.Key,pair.Value);
        }

        if (combinedEditorsData.Any() == false)
        {
            _logger.LogDebug("No custom PropertyEditors found, skipping collection datatype migration data.");
            return;
        }

        using ICoreScope coreScope = _coreScopeProvider.CreateCoreScope(autoComplete: true);

        IEnumerable<IDataType> dataTypes = _dataTypeService.GetAll();

        DataTypeEditorAliasMigrationData[] migrationData = dataTypes
            .Where(dt => combinedEditorsData.ContainsKey(dt.EditorAlias))
            .Select(dt => new DataTypeEditorAliasMigrationData
        {
            DataTypeId = dt.Id,
            EditorAlias = combinedEditorsData[dt.EditorAlias].EditorAlias,
            EditorUiAlias = combinedEditorsData[dt.EditorAlias].EditorUiAlias
        }).ToArray();

        _logger.LogDebug("Collected migration data for {count} DataType(s) that use custom PropertyEditors",migrationData.Length);

        _keyValueService.SetValue("migrateDataEditorSplitCollectionData",_jsonSerializer.Serialize(migrationData));
    }

    private class EditorAliasSplitData
    {
        public EditorAliasSplitData(string originalEditorAlias)
        {
            OriginalEditorAlias = originalEditorAlias;
        }

        public string OriginalEditorAlias { get; init; }
        public string? EditorUiAlias { get; init; }
        public string? EditorAlias { get; init; }
    }

    private class DataTypeEditorAliasMigrationData
    {
        public int DataTypeId { get; set; }
        public string? EditorUiAlias { get; init; }
        public string? EditorAlias { get; init; }
    }

    private string EditorAliasFromValueEditorValueType(string valueType)
    {
        switch (valueType)
        {
            case ValueTypes.Date: return "Umbraco.Plain.DateTime";
            case ValueTypes.DateTime: return "Umbraco.Plain.DateTime";
            case ValueTypes.Decimal: return "Umbraco.Plain.Decimal";
            case ValueTypes.Integer: return "Umbraco.Plain.Integer";
            case ValueTypes.Bigint: return "Umbraco.Plain.Integer";
            case ValueTypes.Json: return "Umbraco.Plain.Json";
            case ValueTypes.Time: return "Umbraco.Plain.Time";
            case ValueTypes.String: return "Umbraco.Plain.String";
            case ValueTypes.Xml: return "Umbraco.Plain.String";
            default:return string.Empty;
        }
    }
}

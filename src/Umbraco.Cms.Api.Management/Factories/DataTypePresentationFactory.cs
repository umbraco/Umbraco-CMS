using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc />
public class DataTypePresentationFactory : IDataTypePresentationFactory
{
    private readonly IDataTypeContainerService _dataTypeContainerService;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IDataValueEditorFactory _dataValueEditorFactory;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DataTypePresentationFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypePresentationFactory"/> class, which is responsible for creating data type presentation models.
    /// </summary>
    /// <param name="dataTypeContainerService">Service used to manage data type containers.</param>
    /// <param name="propertyEditorCollection">A collection containing all available property editors.</param>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors.</param>
    /// <param name="configurationEditorJsonSerializer">Serializer for configuration editor JSON data.</param>
    /// <param name="timeProvider">Provides the current time for time-dependent operations.</param>
    /// <param name="logger">The logger.</param>
    public DataTypePresentationFactory(
        IDataTypeContainerService dataTypeContainerService,
        PropertyEditorCollection propertyEditorCollection,
        IDataValueEditorFactory dataValueEditorFactory,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        TimeProvider timeProvider,
        ILogger<DataTypePresentationFactory> logger)
    {
        _dataTypeContainerService = dataTypeContainerService;
        _propertyEditorCollection = propertyEditorCollection;
        _dataValueEditorFactory = dataValueEditorFactory;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypePresentationFactory"/> class, which is responsible for creating data type presentation models.
    /// </summary>
    /// <param name="dataTypeContainerService">Service used to manage data type containers.</param>
    /// <param name="propertyEditorCollection">A collection containing all available property editors.</param>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors.</param>
    /// <param name="configurationEditorJsonSerializer">Serializer for configuration editor JSON data.</param>
    /// <param name="timeProvider">Provides the current time for time-dependent operations.</param>
    [Obsolete("Please use the constructor that takes all parameters. Scheduled for removal in Umbraco 19.")]
    public DataTypePresentationFactory(
        IDataTypeContainerService dataTypeContainerService,
        PropertyEditorCollection propertyEditorCollection,
        IDataValueEditorFactory dataValueEditorFactory,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        TimeProvider timeProvider)
        : this(
            dataTypeContainerService,
            propertyEditorCollection,
            dataValueEditorFactory,
            configurationEditorJsonSerializer,
            timeProvider,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<DataTypePresentationFactory>>())
    {
    }

    /// <inheritdoc />
    public async Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(CreateDataTypeRequestModel requestModel)
    {
        if (!_propertyEditorCollection.TryGet(requestModel.EditorAlias, out IDataEditor? editor))
        {
            return Attempt.FailWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.PropertyEditorNotFound, new DataType(new VoidEditor(_dataValueEditorFactory), _configurationEditorJsonSerializer));
        }

        Attempt<int, DataTypeOperationStatus> parentAttempt = await GetParentId(requestModel);

        if (parentAttempt.Success == false)
        {
            return Attempt.FailWithStatus<IDataType, DataTypeOperationStatus>(parentAttempt.Status, new DataType(new VoidEditor(_dataValueEditorFactory), _configurationEditorJsonSerializer));
        }

        DateTime createDate = _timeProvider.GetLocalNow().DateTime;
        IDictionary<string, object> configurationData = MapConfigurationData(requestModel, editor);
        var dataType = new DataType(editor, _configurationEditorJsonSerializer)
        {
            Name = requestModel.Name,
            EditorUiAlias = requestModel.EditorUiAlias,
            DatabaseType = GetEditorValueStorageType(editor, configurationData),
            ConfigurationData = configurationData,
            ParentId = parentAttempt.Result,
            CreateDate = createDate,
            UpdateDate = createDate,
        };

        if (requestModel.Id.HasValue)
        {
            dataType.Key = requestModel.Id.Value;
        }

        return Attempt.SucceedWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.Success, dataType);
    }

    private async Task<Attempt<int, DataTypeOperationStatus>> GetParentId(CreateDataTypeRequestModel requestModel)
    {
        if (requestModel.Parent is not null)
        {
            try
            {
                EntityContainer? parent = await _dataTypeContainerService.GetAsync(requestModel.Parent.Id);

                return parent is null
                    ? Attempt.FailWithStatus(DataTypeOperationStatus.ParentNotFound, 0)
                    : Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, parent.Id);
            }
            catch (ArgumentException)
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.ParentNotContainer, 0);
            }
        }

        return Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, Constants.System.Root);
    }

    /// <inheritdoc/>
    public Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(UpdateDataTypeRequestModel requestModel, IDataType current)
    {
        if (!_propertyEditorCollection.TryGet(requestModel.EditorAlias, out IDataEditor? editor))
        {
            return Task.FromResult(Attempt.FailWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.PropertyEditorNotFound, new DataType(new VoidEditor(_dataValueEditorFactory), _configurationEditorJsonSerializer) ));
        }

        var dataType = (IDataType)current.DeepClone();

        IDictionary<string, object> configurationData = MapConfigurationData(requestModel, editor);
        dataType.Name = requestModel.Name;
        dataType.Editor = editor;
        dataType.EditorUiAlias = requestModel.EditorUiAlias;
        dataType.DatabaseType = GetEditorValueStorageType(editor, configurationData);
        dataType.ConfigurationData = configurationData;

        return Task.FromResult(Attempt.SucceedWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.Success, dataType));
    }


    private ValueStorageType GetEditorValueStorageType(IDataEditor editor, IDictionary<string, object> configurationData)
    {
        // Only editors whose configuration object implements IConfigureValueType derive their storage
        // type from the configuration. Building the typed configuration object can throw for editors
        // whose stored configuration doesn't cleanly deserialize into their configuration type; that
        // must not fail the save, so fall back to the value editor's value type in that case.
        try
        {
            if (editor.GetConfigurationEditor().ToConfigurationObject(configurationData, _configurationEditorJsonSerializer)
                is IConfigureValueType configureValueType)
            {
                return ValueTypes.ToStorageType(configureValueType.ValueType);
            }
        }
        catch (Exception ex)
        {
            // Configuration editors are third-party and can throw anything when the stored configuration
            // doesn't deserialize into their configuration type. Fall back to the value editor's value type
            // rather than failing the save, but log so the misconfiguration remains observable.
            _logger.LogWarning(
                ex,
                "Could not build the configuration object for editor {EditorAlias} to determine its value storage type; falling back to the value editor's value type.",
                editor.Alias);
        }

        var valueType = editor.GetValueEditor().ValueType;
        return ValueTypes.ToStorageType(valueType);
    }

    private IDictionary<string, object> MapConfigurationData<T>(T source, IDataEditor editor) where T : DataTypeModelBase
    {
        var configuration = source
            .Values
            .Where(p => p.Value is not null)
            .ToDictionary(p => p.Alias, p => p.Value!);
        IConfigurationEditor? configurationEditor = editor?.GetConfigurationEditor();
        return configurationEditor?.FromConfigurationEditor(configuration)
               ?? new Dictionary<string, object>();
    }

}

using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
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

    public DataTypePresentationFactory(
        IDataTypeContainerService dataTypeContainerService,
        PropertyEditorCollection propertyEditorCollection,
        IDataValueEditorFactory dataValueEditorFactory,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        TimeProvider timeProvider)
    {
        _dataTypeContainerService = dataTypeContainerService;
        _propertyEditorCollection = propertyEditorCollection;
        _dataValueEditorFactory = dataValueEditorFactory;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        _timeProvider = timeProvider;
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
        var dataType = new DataType(editor, _configurationEditorJsonSerializer)
        {
            Name = requestModel.Name,
            EditorUiAlias = requestModel.EditorUiAlias,
            DatabaseType = GetEditorValueStorageType(editor),
            ConfigurationData = MapConfigurationData(requestModel, editor),
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
                var parent = await _dataTypeContainerService.GetAsync(requestModel.Parent.Id);

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

    public Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(UpdateDataTypeRequestModel requestModel, IDataType current)
    {
        if (!_propertyEditorCollection.TryGet(requestModel.EditorAlias, out IDataEditor? editor))
        {
            return Task.FromResult(Attempt.FailWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.PropertyEditorNotFound, new DataType(new VoidEditor(_dataValueEditorFactory), _configurationEditorJsonSerializer) ));
        }

        IDataType dataType = (IDataType)current.DeepClone();

        dataType.Name = requestModel.Name;
        dataType.Editor = editor;
        dataType.EditorUiAlias = requestModel.EditorUiAlias;
        dataType.DatabaseType = GetEditorValueStorageType(editor);
        dataType.ConfigurationData = MapConfigurationData(requestModel, editor);

        return Task.FromResult(Attempt.SucceedWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.Success, dataType));
    }


    private ValueStorageType GetEditorValueStorageType(IDataEditor editor)
    {
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


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
    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IDataValueEditorFactory _dataValueEditorFactory;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    public DataTypePresentationFactory(
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataValueEditorFactory dataValueEditorFactory,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
    {
        _dataTypeService = dataTypeService;
        _propertyEditorCollection = propertyEditorCollection;
        _dataValueEditorFactory = dataValueEditorFactory;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
    }

    /// <inheritdoc />
    public Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(CreateDataTypeRequestModel requestModel)
    {
        if (!_propertyEditorCollection.TryGet(requestModel.PropertyEditorAlias, out IDataEditor? editor))
        {
            return Task.FromResult(Attempt.FailWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.PropertyEditorNotFound, new DataType(new VoidEditor(_dataValueEditorFactory), _configurationEditorJsonSerializer) ));
        }

        int? parentId = requestModel.ParentId.HasValue ? _dataTypeService.GetContainer(requestModel.ParentId.Value)?.ParentId : Constants.System.Root;

        if (parentId is null)
        {
            return Task.FromResult(Attempt.FailWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.ParentNotFound, new DataType(new VoidEditor(_dataValueEditorFactory), _configurationEditorJsonSerializer) ));
        }

        var dataType = new DataType(editor, _configurationEditorJsonSerializer)
        {
            Name = requestModel.Name,
            EditorUiAlias = requestModel.PropertyEditorUiAlias,
            DatabaseType = GetEditorValueStorageType(editor),
            ConfigurationData = MapConfigurationData(requestModel, editor),
            ParentId = parentId.Value,
            CreateDate = DateTime.Now,
        };

        if (requestModel.Id.HasValue)
        {
            dataType.Key = requestModel.Id.Value;
        }


        return Task.FromResult(Attempt.SucceedWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.Success, dataType));
    }

    public Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(UpdateDataTypeRequestModel requestModel, IDataType current)
    {
        if (!_propertyEditorCollection.TryGet(requestModel.PropertyEditorAlias, out IDataEditor? editor))
        {
            return Task.FromResult(Attempt.FailWithStatus<IDataType, DataTypeOperationStatus>(DataTypeOperationStatus.PropertyEditorNotFound, new DataType(new VoidEditor(_dataValueEditorFactory), _configurationEditorJsonSerializer) ));
        }

        IDataType dataType = (IDataType)current.DeepClone();

        dataType.Name = requestModel.Name;
        dataType.Editor = editor;
        dataType.EditorUiAlias = requestModel.PropertyEditorUiAlias;
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

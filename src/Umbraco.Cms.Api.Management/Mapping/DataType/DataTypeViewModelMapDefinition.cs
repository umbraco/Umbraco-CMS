using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Mapping.DataType;

public class DataTypeViewModelMapDefinition : IMapDefinition
{
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IConfigurationEditorJsonSerializer _serializer;
    private readonly IDataTypeService _dataTypeService;

    public DataTypeViewModelMapDefinition(
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer serializer,
        IDataTypeService dataTypeService)
    {
        _propertyEditors = propertyEditors;
        _serializer = serializer;
        _dataTypeService = dataTypeService;
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDataType, DataTypeResponseModel>((_, _) => new DataTypeResponseModel(), Map);

        mapper.Define<UpdateDataTypeRequestModel, IDataType>(InitializeDataType, Map);

        mapper.Define<CreateDataTypeRequestModel, IDataType>(InitializeDataType, Map);

        IDataType InitializeDataType<T>(T source, MapperContext _) where T : DataTypeModelBase =>
            new Core.Models.DataType(_propertyEditors[source.PropertyEditorAlias], _serializer)
            {
                CreateDate = DateTime.Now
            };
    }

    // Umbraco.Code.MapAll
    private void Map(IDataType source, DataTypeResponseModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.ParentKey = _dataTypeService.GetContainer(source.ParentId)?.Key;
        target.Name = source.Name ?? string.Empty;
        target.PropertyEditorAlias = source.EditorAlias;
        target.PropertyEditorUiAlias = source.EditorUiAlias;

        IConfigurationEditor? configurationEditor = source.Editor?.GetConfigurationEditor();
        IDictionary<string, object> configuration = configurationEditor?.ToConfigurationEditor(source.ConfigurationData)
                                         ?? new Dictionary<string, object>();

        target.Values = configuration.Select(c =>
            new DataTypePropertyPresentationModel
            {
                Alias = c.Key,
                Value = c.Value
            }).ToArray();
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate -ParentId
    // Umbraco.Code.MapAll -Id -Key -Path -CreatorId -Level -SortOrder
    private void Map(UpdateDataTypeRequestModel source, IDataType target, MapperContext context)
    {
        IDataEditor editor = GetRequiredDataEditor(source);

        target.Name = source.Name;
        target.Editor = editor;
        target.EditorUiAlias = source.PropertyEditorUiAlias;
        target.DatabaseType = GetEditorValueStorageType(editor);
        target.ConfigurationData = MapConfigurationData(source, editor);
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate
    // Umbraco.Code.MapAll -Id -Key -Path -CreatorId -Level -SortOrder
    private void Map(CreateDataTypeRequestModel source, IDataType target, MapperContext context)
    {
        IDataEditor editor = GetRequiredDataEditor(source);

        target.Name = source.Name;
        target.ParentId = MapParentId(source.ParentKey);
        target.Editor = editor;
        target.EditorUiAlias = source.PropertyEditorUiAlias;
        target.DatabaseType = GetEditorValueStorageType(editor);
        target.ConfigurationData = MapConfigurationData(source, editor);
    }

    private IDataEditor GetRequiredDataEditor<T>(T source) where T : DataTypeModelBase
    {
        if (_propertyEditors.TryGet(source.PropertyEditorAlias, out IDataEditor? editor) == false)
        {
            throw new InvalidOperationException($"Could not find a property editor with alias \"{source.PropertyEditorAlias}\".");
        }

        return editor;
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

    private int MapParentId(Guid? parentKey)
    {
        if (parentKey == null)
        {
            return Constants.System.Root;
        }

        EntityContainer? container = _dataTypeService.GetContainer(parentKey.Value);
        return container?.Id ?? throw new InvalidOperationException($"Could not find a parent container with key \"{parentKey}\".");
    }
}

using Umbraco.Cms.Api.Management.ViewModels.DataType;
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
        mapper.Define<IDataType, DataTypeViewModel>((_, _) => new DataTypeViewModel(), Map);
        mapper.Define<DataTypeViewModel, IDataType>(
            (source, _) =>
                new Core.Models.DataType(_propertyEditors[source.PropertyEditorAlias], _serializer)
                {
                    CreateDate = DateTime.Now
                },
            Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IDataType source, DataTypeViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.ParentKey = _dataTypeService.GetContainer(source.ParentId)?.Key;
        target.Name = source.Name ?? string.Empty;
        target.PropertyEditorAlias = source.EditorAlias;

        IConfigurationEditor? configurationEditor = source.Editor?.GetConfigurationEditor();
        IDictionary<string, object> configuration = configurationEditor?.ToConfigurationEditor(source.ConfigurationData)
                                         ?? new Dictionary<string, object>();

        target.Configuration = configuration.Select(c =>
            new DataTypePropertyViewModel
            {
                Alias = c.Key,
                Value = c.Value
            }).ToArray();
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate
    // Umbraco.Code.MapAll -Id -Key -Path -CreatorId -Level -SortOrder
    private void Map(DataTypeViewModel source, IDataType target, MapperContext context)
    {
        if (_propertyEditors.TryGet(source.PropertyEditorAlias, out IDataEditor? editor) == false)
        {
            throw new InvalidOperationException($"Could not find a property editor with alias \"{source.PropertyEditorAlias}\".");
        }

        target.Name = source.Name;
        target.Editor = editor;
        var valueType = editor.GetValueEditor().ValueType;
        target.DatabaseType = ValueTypes.ToStorageType(valueType);
        var parentId = source.ParentKey.HasValue ? _dataTypeService.GetContainer(source.ParentKey.Value)?.Id : null;
        target.ParentId = parentId ?? target.ParentId;

        var configuration = source
            .Configuration
            .Where(p => p.Value is not null)
            .ToDictionary(p => p.Alias, p => p.Value!);
        IConfigurationEditor? configurationEditor = target.Editor?.GetConfigurationEditor();
        target.ConfigurationData = configurationEditor?.FromConfigurationEditor(configuration)
                                        ?? new Dictionary<string, object>();
    }
}

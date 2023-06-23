using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Mapping.DataType;

public class DataTypeViewModelMapDefinition : IMapDefinition
{
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IDataTypeService _dataTypeService;

    public DataTypeViewModelMapDefinition(
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService)
    {
        _propertyEditors = propertyEditors;
        _dataTypeService = dataTypeService;
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDataType, DataTypeResponseModel>((_, _) => new DataTypeResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IDataType source, DataTypeResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.ParentId = _dataTypeService.GetContainer(source.ParentId)?.Key;
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
}

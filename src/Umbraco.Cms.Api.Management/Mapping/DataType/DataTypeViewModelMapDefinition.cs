using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Mapping.DataType;

public class DataTypeViewModelMapDefinition : IMapDefinition
{
    private readonly IDataTypeService _dataTypeService;

    public DataTypeViewModelMapDefinition(IDataTypeService dataTypeService)
        => _dataTypeService = dataTypeService;

    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IDataType, DataTypeResponseModel>((_, _) => new DataTypeResponseModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(IDataType source, DataTypeResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        Guid? parentId = _dataTypeService.GetContainer(source.ParentId)?.Key;
        target.Parent = ReferenceByIdModel.ReferenceOrNull(parentId);
        target.Name = source.Name ?? string.Empty;
        target.EditorAlias = source.EditorAlias;
        target.EditorUiAlias = source.EditorUiAlias;

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

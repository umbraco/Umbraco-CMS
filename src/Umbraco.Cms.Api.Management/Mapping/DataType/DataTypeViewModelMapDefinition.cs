using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.DataType;

public class DataTypeViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IDataType, DataTypeResponseModel>((_, _) => new DataTypeResponseModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(IDataType source, DataTypeResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.EditorAlias = source.EditorAlias;
        target.EditorUiAlias = source.EditorUiAlias ?? source.EditorAlias;
        target.IsDeletable = source.IsDeletableDataType();
        target.CanIgnoreStartNodes = source.IsBuildInDataType() is false;

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

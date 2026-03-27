using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.DataType;

/// <summary>
/// Provides mapping configuration between data type entities and their corresponding view models in the management API.
/// </summary>
public class DataTypeViewModelMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures object-object mapping definitions for data type view models using the provided mapper.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mappings between <see cref="IDataType"/> and <see cref="DataTypeResponseModel"/>.</param>
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

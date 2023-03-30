using Umbraco.Cms.Api.Management.ViewModels.DataType.Entity;
using Umbraco.Cms.Api.Management.ViewModels.Language.Entity;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Entity;

public class EntityTypeMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<ILanguage, LanguageEntityResponseModel>((_, _) => new LanguageEntityResponseModel(), Map);
        mapper.Define<IDataType, DataTypeEntityResponseModel>((_, _) => new DataTypeEntityResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(ILanguage source, LanguageEntityResponseModel target, MapperContext context)
    {
        target.Name = source.CultureName;
        target.IsoCode = source.IsoCode;
    }

    // Umbraco.Code.MapAll
    private static void Map(IDataType source, DataTypeEntityResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Icon = source.Editor?.Icon ?? Constants.Icons.DataType;
    }
}

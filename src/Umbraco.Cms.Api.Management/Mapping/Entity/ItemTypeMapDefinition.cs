using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary.Item;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Language.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Mapping.Entity;

public class ItemTypeMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<ILanguage, LanguageItemResponseModel>((_, _) => new LanguageItemResponseModel(), Map);
        mapper.Define<IDataType, DataTypeItemResponseModel>((_, _) => new DataTypeItemResponseModel(), Map);
        mapper.Define<IDictionaryItem, DictionaryItemItemResponseModel>((_, _) => new DictionaryItemItemResponseModel(), Map);
        mapper.Define<IDocumentEntitySlim, DocumentItemResponseModel>((_, _) => new DocumentItemResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(ILanguage source, LanguageItemResponseModel target, MapperContext context)
    {
        target.Name = source.CultureName;
        target.IsoCode = source.IsoCode;
    }

    // Umbraco.Code.MapAll
    private static void Map(IDataType source, DataTypeItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = source.Editor?.Icon ?? Constants.Icons.DataType;
    }

    // Umbraco.Code.MapAll
    private static void Map(IDictionaryItem source, DictionaryItemItemResponseModel target, MapperContext context)
    {
        target.Name = source.ItemKey;
        target.Id = source.Key;
        target.Icon = Constants.Icons.Dictionary;
    }

    // Umbraco.Code.MapAll
    private static void Map(IDocumentEntitySlim source, DocumentItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = source.ContentTypeIcon ?? Constants.Icons.ContentType;
    }
}

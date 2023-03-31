using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary.Item;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Language.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Items;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Mapping.Items;

public class ItemTypeMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<ILanguage, LanguageItemResponseModel>((_, _) => new LanguageItemResponseModel(), Map);
        mapper.Define<IDataType, DataTypeItemResponseModel>((_, _) => new DataTypeItemResponseModel(), Map);
        mapper.Define<IDictionaryItem, DictionaryItemItemResponseModel>((_, _) => new DictionaryItemItemResponseModel(), Map);
        mapper.Define<IDocumentEntitySlim, DocumentItemResponseModel>((_, _) => new DocumentItemResponseModel(), Map);
        mapper.Define<IContentType, DocumentTypeItemResponseModel>((_, _) => new DocumentTypeItemResponseModel(), Map);
        mapper.Define<IMediaType, MediaTypeItemResponseModel>((_, _) => new MediaTypeItemResponseModel(), Map);
        mapper.Define<IEntitySlim, MemberGroupItemReponseModel>((_, _) => new MemberGroupItemReponseModel(), Map);
        mapper.Define<IMemberType, MemberTypeItemResponseModel>((_, _) => new MemberTypeItemResponseModel(), Map);
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

    // Umbraco.Code.MapAll
    private static void Map(IContentType source, DocumentTypeItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = source.Icon ?? Constants.Icons.ContentType;
        target.IsElement = source.IsElement;
    }

    // Umbraco.Code.MapAll
    private static void Map(IMediaType source, MediaTypeItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = source.Icon ?? Constants.Icons.ContentType;
    }

    // Umbraco.Code.MapAll
    private static void Map(IEntitySlim source, MemberGroupItemReponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = Constants.Icons.MemberGroup;
    }

    // Umbraco.Code.MapAll
    private static void Map(IMemberType source, MemberTypeItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = source.Icon ?? Constants.Icons.MemberType;
    }
}

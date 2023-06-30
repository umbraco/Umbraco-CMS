using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Language.Item;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Items;
using Umbraco.Cms.Api.Management.ViewModels.RelationType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Mapping.Items;

public class ItemTypeMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<ILanguage, LanguageItemResponseModel>((_, _) => new LanguageItemResponseModel(), Map);
        mapper.Define<IDataType, DataTypeItemResponseModel>((_, _) => new DataTypeItemResponseModel(), Map);
        mapper.Define<IDictionaryItem, DictionaryItemItemResponseModel>((_, _) => new DictionaryItemItemResponseModel(), Map);
        mapper.Define<IContentType, DocumentTypeItemResponseModel>((_, _) => new DocumentTypeItemResponseModel(), Map);
        mapper.Define<IMediaType, MediaTypeItemResponseModel>((_, _) => new MediaTypeItemResponseModel(), Map);
        mapper.Define<IEntitySlim, MemberGroupItemResponseModel>((_, _) => new MemberGroupItemResponseModel(), Map);
        mapper.Define<ITemplate, TemplateItemResponseModel>((_, _) => new TemplateItemResponseModel { Alias = string.Empty }, Map);
        mapper.Define<IMemberType, MemberTypeItemResponseModel>((_, _) => new MemberTypeItemResponseModel(), Map);
        mapper.Define<IRelationType, RelationTypeItemResponseModel>((_, _) => new RelationTypeItemResponseModel(), Map);
        mapper.Define<IMediaEntitySlim, MediaItemResponseModel>((_, _) => new MediaItemResponseModel(), Map);
        mapper.Define<IMember, MemberItemResponseModel>((_, _) => new MemberItemResponseModel(), Map);
        mapper.Define<IUser, UserItemResponseModel>((_, _) => new UserItemResponseModel(), Map);
        mapper.Define<IUserGroup, UserGroupItemResponseModel>((_, _) => new UserGroupItemResponseModel(), Map);
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
        target.Icon = source.Editor?.Icon;
    }

    // Umbraco.Code.MapAll
    private static void Map(IDictionaryItem source, DictionaryItemItemResponseModel target, MapperContext context)
    {
        target.Name = source.ItemKey;
        target.Id = source.Key;
    }

    // Umbraco.Code.MapAll
    private static void Map(IContentType source, DocumentTypeItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = source.Icon;
        target.IsElement = source.IsElement;
    }

    // Umbraco.Code.MapAll
    private static void Map(IMediaType source, MediaTypeItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = source.Icon;
    }

    // Umbraco.Code.MapAll
    private static void Map(IEntitySlim source, MemberGroupItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
    }

    // Umbraco.Code.MapAll
    private static void Map(ITemplate source, TemplateItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Alias = source.Alias;
    }

    // Umbraco.Code.MapAll
    private static void Map(IMemberType source, MemberTypeItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? string.Empty;
        target.Id = source.Key;
        target.Icon = source.Icon;
    }

    // Umbraco.Code.MapAll
    private static void Map(IRelationType source, RelationTypeItemResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
    }

    // Umbraco.Code.MapAll
    private static void Map(IMediaEntitySlim source, MediaItemResponseModel target, MapperContext context)
    {
        target.Icon = source.ContentTypeIcon;
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
    }

    // Umbraco.Code.MapAll
    private static void Map(IMember source, MemberItemResponseModel target, MapperContext context)
    {
        target.Icon = source.ContentType.Icon;
        target.Id = source.Key;
        target.Name = source.Name ?? source.Username;
    }

    // Umbraco.Code.MapAll
    private static void Map(IUser source, UserItemResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? source.Username;
    }

    // Umbraco.Code.MapAll
    private static void Map(IUserGroup source, UserGroupItemResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? source.Alias;
        target.Icon = source.Icon;
    }
}

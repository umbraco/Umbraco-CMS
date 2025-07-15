using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Language.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Api.Management.ViewModels.RelationType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Item;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Item;

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
        mapper.Define<IUserGroup, UserGroupItemResponseModel>((_, _) => new UserGroupItemResponseModel(), Map);
        mapper.Define<IWebhook, WebhookItemResponseModel>((_, _) => new WebhookItemResponseModel(), Map);
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
        target.EditorUiAlias = source.EditorUiAlias;
        target.EditorAlias = source.EditorAlias;
        target.IsDeletable = source.IsDeletableDataType();
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
        target.Description = source.Description;
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
        target.IsDeletable = source.IsDeletableRelationType();
    }

    // Umbraco.Code.MapAll
    private static void Map(IUserGroup source, UserGroupItemResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? source.Alias;
        target.Icon = source.Icon;
        target.Alias = source.Alias;
    }

    // Umbraco.Code.MapAll
    private static void Map(IWebhook source, WebhookItemResponseModel target, MapperContext context)
    {
        target.Name = source.Name ?? source.Url;
        target.Url = source.Url;
        target.Enabled = source.Enabled;
        target.Events = string.Join(",", source.Events);
        target.Types = string.Join(",", source.ContentTypeKeys);
    }
}

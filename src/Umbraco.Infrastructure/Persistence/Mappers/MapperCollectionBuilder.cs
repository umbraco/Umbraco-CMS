using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

public class MapperCollectionBuilder : SetCollectionBuilderBase<MapperCollectionBuilder, MapperCollection, BaseMapper>
{
    protected override MapperCollectionBuilder This => this;

    public override void RegisterWith(IServiceCollection services)
    {
        base.RegisterWith(services);

        // default initializer registers
        // - service MapperCollectionBuilder, returns MapperCollectionBuilder
        // - service MapperCollection, returns MapperCollectionBuilder's collection
        // we want to register extra
        // - service IMapperCollection, returns MappersCollectionBuilder's collection
        services.AddSingleton<MapperConfigurationStore>();
        services.AddSingleton<IMapperCollection>(factory => factory.GetRequiredService<MapperCollection>());
    }

    public MapperCollectionBuilder AddCoreMappers()
    {
        Add<AccessMapper>();
        Add<AuditItemMapper>();
        Add<ContentMapper>();
        Add<ContentTypeMapper>();
        Add<SimpleContentTypeMapper>();
        Add<DataTypeMapper>();
        Add<DictionaryMapper>();
        Add<DictionaryTranslationMapper>();
        Add<DomainMapper>();
        Add<KeyValueMapper>();
        Add<LanguageMapper>();
        Add<MacroMapper>();
        Add<MediaMapper>();
        Add<MediaTypeMapper>();
        Add<MemberGroupMapper>();
        Add<MemberMapper>();
        Add<MemberTypeMapper>();
        Add<PropertyGroupMapper>();
        Add<PropertyMapper>();
        Add<PropertyTypeMapper>();
        Add<RelationMapper>();
        Add<RelationTypeMapper>();
        Add<ServerRegistrationMapper>();
        Add<TagMapper>();
        Add<TemplateMapper>();
        Add<UmbracoEntityMapper>();
        Add<UserMapper>();
        Add<ExternalLoginMapper>();
        Add<ExternalLoginTokenMapper>();
        Add<UserGroupMapper>();
        Add<AuditEntryMapper>();
        Add<ConsentMapper>();
        Add<LogViewerQueryMapper>();
        return this;
    }
}

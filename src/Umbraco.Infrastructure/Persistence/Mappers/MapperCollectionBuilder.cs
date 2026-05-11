using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides a builder for creating a collection of database mappers used in Umbraco persistence operations.
/// </summary>
public class MapperCollectionBuilder : SetCollectionBuilderBase<MapperCollectionBuilder, MapperCollection, BaseMapper>
{
    protected override MapperCollectionBuilder This => this;

    /// <summary>
    /// Registers the mapper collection and related services with the specified <see cref="IServiceCollection"/>.
    /// This includes the <see cref="MapperConfigurationStore"/>, <see cref="IMapperCollection"/>, and ensures that the mapper collection is available for dependency injection.
    /// </summary>
    /// <param name="services">The service collection to register the mapper-related services with.</param>
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

    /// <summary>
    /// Registers the predefined set of core mappers with the collection.
    /// </summary>
    /// <returns>
    /// The current <see cref="Umbraco.Cms.Infrastructure.Persistence.Mappers.MapperCollectionBuilder"/> instance, enabling method chaining.
    /// </returns>
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

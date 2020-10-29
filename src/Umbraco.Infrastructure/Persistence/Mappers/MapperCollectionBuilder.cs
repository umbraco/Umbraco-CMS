using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Infrastructure.Persistence.Mappers;

namespace Umbraco.Core.Persistence.Mappers
{
    public class MapperCollectionBuilder : SetCollectionBuilderBase<MapperCollectionBuilder, MapperCollection, BaseMapper>
    {
        protected override MapperCollectionBuilder This => this;

        public override void RegisterWith(IRegister register)
        {
            base.RegisterWith(register);

            // default initializer registers
            // - service MapperCollectionBuilder, returns MapperCollectionBuilder
            // - service MapperCollection, returns MapperCollectionBuilder's collection
            // we want to register extra
            // - service IMapperCollection, returns MappersCollectionBuilder's collection

            register.Register<MapperConfigurationStore>(Lifetime.Singleton);
            register.Register<IMapperCollection>(factory => factory.GetRequiredService<MapperCollection>());
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
            Add<UserGroupMapper>();
            Add<AuditEntryMapper>();
            Add<ConsentMapper>();
            return this;
        }
    }
}

﻿using System;
using System.Collections.Concurrent;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Persistence.Mappers
{
    public class MapperCollectionBuilder : SetCollectionBuilderBase<MapperCollectionBuilder, MapperCollection, BaseMapper>
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> _maps
            = new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>();

        protected override MapperCollectionBuilder This => this;

        public override void RegisterWith(IRegister register)
        {
            base.RegisterWith(register);

            // default initializer registers
            // - service MapperCollectionBuilder, returns MapperCollectionBuilder
            // - service MapperCollection, returns MapperCollectionBuilder's collection
            // we want to register extra
            // - service IMapperCollection, returns MappersCollectionBuilder's collection

            register.Register<IMapperCollection>(factory => factory.GetInstance<MapperCollection>());
        }

        protected override BaseMapper CreateItem(IFactory factory, Type itemType)
        {
            return (BaseMapper) factory.CreateInstance(itemType, _maps);
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

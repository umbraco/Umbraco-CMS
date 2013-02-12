using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Persistence.Mappers
{
    internal static class MappingResolver
    {
        internal static BaseMapper ResolveMapperByType(Type type)
        {
            if (type == typeof (ServerRegistration))
                return ServerRegistrationMapper.Instance;

            if (type == typeof (IContent) || type == typeof (Content))
                return ContentMapper.Instance;

            if (type == typeof (IContentType) || type == typeof (ContentType))
                return ContentTypeMapper.Instance;

            if (type == typeof(IDataTypeDefinition) || type == typeof(DataTypeDefinition))
                return DataTypeDefinitionMapper.Instance;

            if (type == typeof(IDictionaryItem) || type == typeof(DictionaryItem))
                return DictionaryMapper.Instance;

            if (type == typeof(IDictionaryTranslation) || type == typeof(DictionaryTranslation))
                return DictionaryTranslationMapper.Instance;

            if (type == typeof(ILanguage) || type == typeof(Language))
                return LanguageMapper.Instance;

            if (type == typeof(IMedia) || type == typeof(Models.Media))
                return MediaMapper.Instance;

            if (type == typeof(IMediaType) || type == typeof(MediaType))
                return MediaTypeMapper.Instance;

            if (type == typeof(PropertyGroup))
                return PropertyGroupMapper.Instance;

            if (type == typeof(Property))
                return PropertyMapper.Instance;

            if (type == typeof(PropertyType))
                return PropertyTypeMapper.Instance;

            if (type == typeof(Relation))
                return RelationMapper.Instance;

            if (type == typeof(RelationType))
                return RelationTypeMapper.Instance;

            if (type == typeof(IUser) || type == typeof(User))
                return UserMapper.Instance;

            if (type == typeof(IUserType) || type == typeof(UserType))
                return UserTypeMapper.Instance;

            throw new Exception("Invalid Type: A Mapper could not be resolved based on the passed in Type");
        }

        internal static string GetMapping(Type type, string propertyName)
        {
            var mapper = ResolveMapperByType(type);
            var result = mapper.Map(propertyName);
            if(string.IsNullOrEmpty(result))
                throw new Exception("Invalid mapping: The passed in property name could not be mapped using the passed in type");

            return result;
        }
    }
}
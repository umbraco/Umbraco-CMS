using System;
using System.Reflection;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Public api models to DTO mapper used by PetaPoco to map Properties to Columns
    /// </summary>
    internal class ModelDtoMapper : IMapper
    {
        public void GetTableInfo(Type t, TableInfo ti)
        { }

        public bool MapPropertyToColumn(PropertyInfo pi, ref string columnName, ref bool resultColumn)
        {
            if (pi.DeclaringType == typeof(Content) || pi.DeclaringType == typeof(IContent))
            {
                var mappedName = ContentMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            if (pi.DeclaringType == typeof(Models.Media) || pi.DeclaringType == typeof(IMedia))
            {
                var mappedName = MediaMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            if (pi.DeclaringType == typeof(ContentType) || pi.DeclaringType == typeof(IContentType) || pi.DeclaringType == typeof(IMediaType))
            {
                var mappedName = ContentTypeMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (pi.DeclaringType == typeof(DataTypeDefinition) || pi.DeclaringType == typeof(IDataTypeDefinition))
            {
                var mappedName = DataTypeDefinitionMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            if (pi.DeclaringType == typeof(DictionaryItem) || pi.DeclaringType == typeof(IDictionaryItem))
            {
                var mappedName = DictionaryMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            if (pi.DeclaringType == typeof(DictionaryTranslation) || pi.DeclaringType == typeof(IDictionaryTranslation))
            {
                var mappedName = DictionaryTranslationMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            if (pi.DeclaringType == typeof(Language) || pi.DeclaringType == typeof(ILanguage))
            {
                var mappedName = LanguageMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            if (pi.DeclaringType == typeof(Relation))
            {
                var mappedName = RelationMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (pi.DeclaringType == typeof(RelationType))
            {
                var mappedName = RelationTypeMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            if (pi.DeclaringType == typeof(PropertyType))
            {
                var mappedName = PropertyTypeMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            if (pi.DeclaringType == typeof(PropertyGroup))
            {
                var mappedName = PropertyGroupMapper.Instance.Map(pi.Name);
                if (mappedName == string.Empty)
                    return false;

                columnName = mappedName;
                return true;
            }

            return true;
        }

        public Func<object, object> GetFromDbConverter(PropertyInfo pi, Type sourceType)
        {
            return null;
        }

        public Func<object, object> GetToDbConverter(Type sourceType)
        {
            return null;
        }
    }
}
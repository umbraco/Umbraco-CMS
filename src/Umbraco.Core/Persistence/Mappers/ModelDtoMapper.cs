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

        public bool MapPropertyToColumn(Type t, PropertyInfo pi, ref string columnName, ref bool resultColumn)
        {
            if (t == typeof(Content) || t == typeof(IContent))
            {
                var mappedName = ContentMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(Models.Media) || t == typeof(IMedia))
            {
                var mappedName = MediaMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(ContentType) || t == typeof(IContentType) || t == typeof(IMediaType))
            {
                var mappedName = ContentTypeMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(DataTypeDefinition) || t == typeof(IDataTypeDefinition))
            {
                var mappedName = DataTypeDefinitionMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(DictionaryItem) || t == typeof(IDictionaryItem))
            {
                var mappedName = DictionaryMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(DictionaryTranslation) || t == typeof(IDictionaryTranslation))
            {
                var mappedName = DictionaryTranslationMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(Language) || t == typeof(ILanguage))
            {
                var mappedName = LanguageMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(Relation))
            {
                var mappedName = RelationMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(RelationType))
            {
                var mappedName = RelationTypeMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(PropertyType))
            {
                var mappedName = PropertyTypeMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
                return true;
            }

            if (t == typeof(PropertyGroup))
            {
                var mappedName = PropertyGroupMapper.Instance.Map(pi.Name);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    columnName = mappedName;
                }
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
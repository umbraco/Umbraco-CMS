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
                columnName = ContentMapper.Instance.Map(pi.Name);
                return true;
            }

            if (pi.DeclaringType == typeof(Models.Media) || pi.DeclaringType == typeof(IMedia))
            {
                columnName = MediaMapper.Instance.Map(pi.Name);
                return true;
            }

            if (pi.DeclaringType == typeof(ContentType) || pi.DeclaringType == typeof(IContentType) || pi.DeclaringType == typeof(IMediaType))
            {
                columnName = ContentTypeMapper.Instance.Map(pi.Name);
            }

            if (pi.DeclaringType == typeof(DataTypeDefinition))
            {
                columnName = DataTypeDefinitionMapper.Instance.Map(pi.Name);
            }

            if (pi.DeclaringType == typeof(DictionaryItem))
            {
                columnName = DictionaryMapper.Instance.Map(pi.Name);
            }

            if (pi.DeclaringType == typeof(DictionaryTranslation))
            {
                columnName = DictionaryTranslationMapper.Instance.Map(pi.Name);
            }

            if (pi.DeclaringType == typeof(Language))
            {
                columnName = LanguageMapper.Instance.Map(pi.Name);
            }

            if (pi.DeclaringType == typeof(Relation))
            {
                columnName = RelationMapper.Instance.Map(pi.Name);
            }

            if (pi.DeclaringType == typeof(RelationType))
            {
                columnName = RelationTypeMapper.Instance.Map(pi.Name);
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
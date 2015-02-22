using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Tag"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(Tag))]
    [MapperFor(typeof(ITag))]
    public sealed class TagMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();


        #region Overrides of BaseMapper

        public TagMapper(ISqlSyntaxProvider sqlSyntax) : base(sqlSyntax)
        {
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty)
            {
                CacheMap<Tag, TagDto>(src => src.Id, dto => dto.Id);
                CacheMap<Tag, TagDto>(src => src.Text, dto => dto.Tag);
                CacheMap<Tag, TagDto>(src => src.Group, dto => dto.Group);
            }
        }

        #endregion
    }
}

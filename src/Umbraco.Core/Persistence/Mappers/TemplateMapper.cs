using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Template"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(Template))]
    [MapperFor(typeof(ITemplate))]
    public sealed class TemplateMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public TemplateMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        internal override void BuildMap()
        {
            if(PropertyInfoCache.IsEmpty)
            {
                CacheMap<Template, TemplateDto>(src => src.Id, dto => dto.NodeId);
                CacheMap<Template, NodeDto>(src => src.MasterTemplateId, dto => dto.ParentId);
                CacheMap<Template, NodeDto>(src => src.Key, dto => dto.UniqueId);
                CacheMap<Template, TemplateDto>(src => src.Alias, dto => dto.Alias);
                CacheMap<Template, TemplateDto>(src => src.Content, dto => dto.Design);
            }
        }
        
        #endregion
    }
}

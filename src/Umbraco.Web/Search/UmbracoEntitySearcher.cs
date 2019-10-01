using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Search
{
    public sealed class UmbracoEntitySearcher
    {
        private readonly IEntityService _entityService;
        private readonly ISqlContext _sqlContext;
        private readonly UmbracoMapper _mapper;

        public UmbracoEntitySearcher(IEntityService entityService, ISqlContext sqlContext, UmbracoMapper mapper)
        {
            _entityService = entityService;
            _sqlContext = sqlContext;
            _mapper = mapper;
        }

        public IEnumerable<SearchResultEntity> Search(UmbracoObjectTypes objectType, string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            //if it's a GUID, match it
            Guid.TryParse(query, out var g);

            var results = _entityService.GetPagedDescendants(objectType, pageIndex, pageSize, out totalFound,
                filter: _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(query) || x.Key == g));
            return _mapper.MapEnumerable<IEntitySlim, SearchResultEntity>(results);
        }
    }
}

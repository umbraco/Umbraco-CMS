using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class TestRepository : EntityRepositoryBase<int, ITest>, ITestRepository
    {
        public TestRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<TestRepository> logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return isCount ? Sql().SelectCount().From<TestDto>() : GetBaseQuery();
        }

        private Sql<ISqlContext> GetBaseQuery()
        {
            return Sql().Select<TestDto>().From<TestDto>();
        }

        protected override string GetBaseWhereClause()
        {
            return "id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {            
            return new List<string>();
        }

        protected override ITest PerformGet(int id) => throw new NotImplementedException();

        protected override IEnumerable<ITest> PerformGetAll(params int[] ids)
        {
            var dtos = ids.Length == 0
                ? Database.Fetch<TestDto>(Sql().Select<TestDto>().From<TestDto>())
                : Database.FetchByGroups<TestDto, int>(ids, 2000, batch => Sql().Select<TestDto>().From<TestDto>().WhereIn<TestDto>(x => x.Id, batch));

            return dtos.Select(TestFactory.BuildEntity).ToList();
        }

        protected override IEnumerable<ITest> PerformGetByQuery(IQuery<ITest> query) => throw new NotImplementedException();
        protected override void PersistNewItem(ITest item) => throw new NotImplementedException();
        protected override void PersistUpdatedItem(ITest item) => throw new NotImplementedException();
    }
}

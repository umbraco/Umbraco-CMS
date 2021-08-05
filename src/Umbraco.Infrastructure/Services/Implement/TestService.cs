using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Implement
{
    public class TestService : RepositoryService, ITestService
    {
        private readonly ITestRepository _testRepository;

        public TestService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            ITestRepository testRepository)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _testRepository = testRepository;
        }

        public IEnumerable<ITest> GetAll()
        {
            using (ScopeProvider.CreateScope(autoComplete: true, connectionStringAlias: "Test"))
            {
                return _testRepository.GetMany();
            }
        }
    }
}

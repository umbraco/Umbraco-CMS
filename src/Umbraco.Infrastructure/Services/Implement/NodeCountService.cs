using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services.Implement
{
    public class NodeCountService : INodeCountService
    {
        private readonly IScopeProvider _scopeProvider;

        public NodeCountService(IScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

        public int GetNodeCount(Guid nodeType)
        {
            int count = 0;
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var query = scope.Database.SqlContext.Sql()
                    .SelectCount()
                    .From<NodeDto>()
                    .Where<NodeDto>(x => x.NodeObjectType == nodeType);

                count = scope.Database.ExecuteScalar<int>(query);
            }

            return count;
        }
    }
}

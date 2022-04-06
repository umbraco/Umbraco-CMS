using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
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
            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var query = scope.Database.SqlContext.Sql()
                    .SelectCount()
                    .From<NodeDto>()
                    .Where<NodeDto>(x => x.NodeObjectType == nodeType && x.Trashed == false);

                count = scope.Database.ExecuteScalar<int>(query);
            }

            return count;
        }

        public int GetMediaCount()
        {
            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var query = scope.Database.SqlContext.Sql()
                    .SelectCount()
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>()
                    .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>()
                    .On<ContentDto, ContentTypeDto>(left => left.ContentTypeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media)
                    .Where<NodeDto>(x => !x.Trashed)
                    .Where<ContentTypeDto>(x => x.Alias != Constants.Conventions.MediaTypes.Folder);

                return scope.Database.ExecuteScalar<int>(query);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class DocumentVersionRepository : IDocumentVersionRepository
    {
        private readonly IScopeAccessor _scopeAccessor;

        public DocumentVersionRepository(IScopeAccessor scopeAccessor)
        {
            _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
        }

        /// <inheritdoc />
        /// <remarks>
        /// Never includes current draft version. <br/>
        /// Never includes current published version.<br/>
        /// Never includes versions marked as "preventCleanup".<br/>
        /// </remarks>
        public IReadOnlyCollection<HistoricContentVersionMeta> GetDocumentVersionsEligibleForCleanup()
        {
            var query = _scopeAccessor.AmbientScope.SqlContext.Sql();

            query.Select(@"umbracoDocument.nodeId as contentId,
                           umbracoContent.contentTypeId as contentTypeId,
                           umbracoContentVersion.id as versionId,
                           umbracoContentVersion.versionDate as versionDate")
                .From<DocumentDto>()
                .InnerJoin<ContentDto>()
                    .On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentVersionDto>()
                    .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<DocumentVersionDto>()
                    .On<ContentVersionDto, DocumentVersionDto>(left => left.Id, right => right.Id)
                .Where<ContentVersionDto>(x => !x.Current) // Never delete current draft version
                .Where<ContentVersionDto>(x => !x.PreventCleanup) // Never delete "pinned" versions
                .Where<DocumentVersionDto>(x => !x.Published); // Never delete published version

            return _scopeAccessor.AmbientScope.Database.Fetch<HistoricContentVersionMeta>(query);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ContentVersionCleanupPolicySettings> GetCleanupPolicies()
        {
            var query = _scopeAccessor.AmbientScope.SqlContext.Sql();

            query.Select<ContentVersionCleanupPolicyDto>()
                .From<ContentVersionCleanupPolicyDto>();

            return _scopeAccessor.AmbientScope.Database.Fetch<ContentVersionCleanupPolicySettings>(query);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Deletes in batches of <see cref="Constants.Sql.MaxParameterCount"/>
        /// </remarks>
        public void DeleteVersions(IEnumerable<int> versionIds)
        {
            foreach (var group in versionIds.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                var groupedVersionIds = group.ToList();

                // Note: We had discussed doing this in a single SQL Command.
                // If you can work out how to make that work with SQL CE, let me know!
                // Can use test PerformContentVersionCleanup_WithNoKeepPeriods_DeletesEverythingExceptActive to try things out.

                var query = _scopeAccessor.AmbientScope.SqlContext.Sql()
                    .Delete<PropertyDataDto>()
                    .WhereIn<PropertyDataDto>(x => x.VersionId, groupedVersionIds);
                _scopeAccessor.AmbientScope.Database.Execute(query);

                query = _scopeAccessor.AmbientScope.SqlContext.Sql()
                    .Delete<ContentVersionCultureVariationDto>()
                    .WhereIn<ContentVersionCultureVariationDto>(x => x.VersionId, groupedVersionIds);
                _scopeAccessor.AmbientScope.Database.Execute(query);

                query = _scopeAccessor.AmbientScope.SqlContext.Sql()
                    .Delete<DocumentVersionDto>()
                    .WhereIn<DocumentVersionDto>(x => x.Id, groupedVersionIds);
                _scopeAccessor.AmbientScope.Database.Execute(query);

                query = _scopeAccessor.AmbientScope.SqlContext.Sql()
                    .Delete<ContentVersionDto>()
                    .WhereIn<ContentVersionDto>(x => x.Id, groupedVersionIds);
                _scopeAccessor.AmbientScope.Database.Execute(query);
            }
        }
    }
}

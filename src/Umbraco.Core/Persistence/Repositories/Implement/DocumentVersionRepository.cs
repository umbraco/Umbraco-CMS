﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
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
        public IReadOnlyCollection<ContentVersionMeta> GetDocumentVersionsEligibleForCleanup()
        {
            var query = _scopeAccessor.AmbientScope.SqlContext.Sql();

            query.Select(@"umbracoDocument.nodeId as contentId,
                           umbracoContent.contentTypeId as contentTypeId,
                           umbracoContentVersion.id as versionId,
                           umbracoContentVersion.userId as userId,
                           umbracoContentVersion.versionDate as versionDate,
                           umbracoDocumentVersion.published as currentPublishedVersion,
                           umbracoContentVersion.[current] as currentDraftVersion,
                           umbracoContentVersion.preventCleanup as preventCleanup,
                           umbracoUser.userName as username")
                .From<DocumentDto>()
                .InnerJoin<ContentDto>()
                    .On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentVersionDto>()
                    .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<DocumentVersionDto>()
                    .On<ContentVersionDto, DocumentVersionDto>(left => left.Id, right => right.Id)
                .LeftJoin<UserDto>()
                    .On<UserDto, ContentVersionDto>(left => left.Id, right => right.UserId)
                .Where<ContentVersionDto>(x => !x.Current) // Never delete current draft version
                .Where<ContentVersionDto>(x => !x.PreventCleanup) // Never delete "pinned" versions
                .Where<DocumentVersionDto>(x => !x.Published); // Never delete published version

            return _scopeAccessor.AmbientScope.Database.Fetch<ContentVersionMeta>(query);
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
        public IEnumerable<ContentVersionMeta> GetPagedItemsByContentId(int contentId, long pageIndex, int pageSize, out long totalRecords, int? languageId = null)
        {
            var query = _scopeAccessor.AmbientScope.SqlContext.Sql();

            query.Select(@"umbracoDocument.nodeId as contentId,
                           umbracoContent.contentTypeId as contentTypeId,
                           umbracoContentVersion.id as versionId,
                           umbracoContentVersion.userId as userId,
                           umbracoContentVersion.versionDate as versionDate,
                           umbracoDocumentVersion.published as currentPublishedVersion,
                           umbracoContentVersion.[current] as currentDraftVersion,
                           umbracoContentVersion.preventCleanup as preventCleanup,
                           umbracoUser.userName as username")
                .From<DocumentDto>()
                .InnerJoin<ContentDto>()
                    .On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentVersionDto>()
                    .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<DocumentVersionDto>()
                    .On<ContentVersionDto, DocumentVersionDto>(left => left.Id, right => right.Id)
                .LeftJoin<UserDto>()
                    .On<UserDto, ContentVersionDto>(left => left.Id, right => right.UserId)
                .LeftJoin<ContentVersionCultureVariationDto>()
                    .On<ContentVersionCultureVariationDto, ContentVersionDto>(left => left.VersionId, right => right.Id)
                .Where<ContentVersionDto>(x => x.NodeId == contentId);

            // TODO: If there's not a better way to write this then we need a better way to write this.
            query = languageId.HasValue
                ? query.Where<ContentVersionCultureVariationDto>(x => x.LanguageId == languageId.Value)
                : query.Where("umbracoContentVersionCultureVariation.languageId is null");

            query = query.OrderByDescending<ContentVersionDto>(x => x.Id);

            var page = _scopeAccessor.AmbientScope.Database.Page<ContentVersionMeta>(pageIndex + 1, pageSize, query);

            totalRecords = page.TotalItems;

            return page.Items;
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

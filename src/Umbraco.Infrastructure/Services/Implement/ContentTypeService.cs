using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    public class ContentTypeService : ContentTypeServiceBase<IContentTypeRepository, IContentType>, IContentTypeService
    {
        public ContentTypeService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IContentService contentService,
            IContentTypeRepository repository, IAuditRepository auditRepository, IDocumentTypeContainerRepository entityContainerRepository, IEntityRepository entityRepository,
            IEventAggregator eventAggregator)
            : base(provider, loggerFactory, eventMessagesFactory, repository, auditRepository, entityContainerRepository, entityRepository, eventAggregator)
        {
            ContentService = contentService;
        }

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Cms.Core.Constants.Locks.ContentTypes };
        protected override int[] WriteLockIds { get; } = { Cms.Core.Constants.Locks.ContentTree, Cms.Core.Constants.Locks.ContentTypes };

        private IContentService ContentService { get; }

        protected override Guid ContainedObjectType => Cms.Core.Constants.ObjectTypes.DocumentType;

        #region Notifications

        protected override SavingNotification<IContentType> GetSavingNotification(IContentType item,
            EventMessages eventMessages) => new ContentTypeSavingNotification(item, eventMessages);

        protected override SavingNotification<IContentType> GetSavingNotification(IEnumerable<IContentType> items,
            EventMessages eventMessages) => new ContentTypeSavingNotification(items, eventMessages);

        protected override SavedNotification<IContentType> GetSavedNotification(IContentType item,
            EventMessages eventMessages) => new ContentTypeSavedNotification(item, eventMessages);

        protected override SavedNotification<IContentType> GetSavedNotification(IEnumerable<IContentType> items,
            EventMessages eventMessages) => new ContentTypeSavedNotification(items, eventMessages);

        protected override DeletingNotification<IContentType> GetDeletingNotification(IContentType item,
            EventMessages eventMessages) => new ContentTypeDeletingNotification(item, eventMessages);

        protected override DeletingNotification<IContentType> GetDeletingNotification(IEnumerable<IContentType> items,
            EventMessages eventMessages) => new ContentTypeDeletingNotification(items, eventMessages);

        protected override DeletedNotification<IContentType> GetDeletedNotification(IEnumerable<IContentType> items,
            EventMessages eventMessages) => new ContentTypeDeletedNotification(items, eventMessages);

        protected override MovingNotification<IContentType> GetMovingNotification(MoveEventInfo<IContentType> moveInfo,
            EventMessages eventMessages) => new ContentTypeMovingNotification(moveInfo, eventMessages);

        protected override MovedNotification<IContentType> GetMovedNotification(
            IEnumerable<MoveEventInfo<IContentType>> moveInfo, EventMessages eventMessages) =>
            new ContentTypeMovedNotification(moveInfo, eventMessages);

        protected override ContentTypeChangeNotification<IContentType> GetContentTypeChangedNotification(
            IEnumerable<ContentTypeChange<IContentType>> changes, EventMessages eventMessages) =>
            new ContentTypeChangedNotification(changes, eventMessages);

        protected override ContentTypeRefreshNotification<IContentType> GetContentTypeRefreshedNotification(
            IEnumerable<ContentTypeChange<IContentType>> changes, EventMessages eventMessages) =>
            new ContentTypeRefreshedNotification(changes, eventMessages);

        #endregion

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var typeIdsA = typeIds.ToArray();
                ContentService.DeleteOfTypes(typeIdsA);
                ContentService.DeleteBlueprintsOfTypes(typeIdsA);
                scope.Complete();
            }
        }

        /// <summary>
        /// Gets all property type aliases across content, media and member types.
        /// </summary>
        /// <returns>All property type aliases.</returns>
        /// <remarks>Beware! Works across content, media and member types.</remarks>
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                // that one is special because it works across content, media and member types
                scope.ReadLock(new[] { Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes });
                return Repository.GetAllPropertyTypeAliases();
            }
        }

        /// <summary>
        /// Gets all content type aliases across content, media and member types.
        /// </summary>
        /// <param name="guids">Optional object types guid to restrict to content, and/or media, and/or member types.</param>
        /// <returns>All content type aliases.</returns>
        /// <remarks>Beware! Works across content, media and member types.</remarks>
        public IEnumerable<string> GetAllContentTypeAliases(params Guid[] guids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                // that one is special because it works across content, media and member types
                scope.ReadLock(new[] { Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes });
                return Repository.GetAllContentTypeAliases(guids);
            }
        }

        /// <summary>
        /// Gets all content type id for aliases across content, media and member types.
        /// </summary>
        /// <param name="aliases">Aliases to look for.</param>
        /// <returns>All content type ids.</returns>
        /// <remarks>Beware! Works across content, media and member types.</remarks>
        public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                // that one is special because it works across content, media and member types
                scope.ReadLock(new[] { Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes });
                return Repository.GetAllContentTypeIds(aliases);
            }
        }
    }
}

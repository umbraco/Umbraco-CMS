using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    public class ContentTypeService : ContentTypeServiceBase<IContentTypeRepository, IContentType, IContentTypeService>, IContentTypeService
    {
        public ContentTypeService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IContentService contentService,
            IContentTypeRepository repository, IAuditRepository auditRepository, IDocumentTypeContainerRepository entityContainerRepository, IEntityRepository entityRepository)
            : base(provider, loggerFactory, eventMessagesFactory, repository, auditRepository, entityContainerRepository, entityRepository)
        {
            ContentService = contentService;
        }

        protected override IContentTypeService This => this;

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Cms.Core.Constants.Locks.ContentTypes };
        protected override int[] WriteLockIds { get; } = { Cms.Core.Constants.Locks.ContentTree, Cms.Core.Constants.Locks.ContentTypes };

        private IContentService ContentService { get; }

        protected override Guid ContainedObjectType => Cms.Core.Constants.ObjectTypes.DocumentType;

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
                scope.ReadLock(Cms.Core.Constants.Locks.ContentTypes, Cms.Core.Constants.Locks.MediaTypes, Cms.Core.Constants.Locks.MemberTypes);
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
                scope.ReadLock(Cms.Core.Constants.Locks.ContentTypes, Cms.Core.Constants.Locks.MediaTypes, Cms.Core.Constants.Locks.MemberTypes);
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
                scope.ReadLock(Cms.Core.Constants.Locks.ContentTypes, Cms.Core.Constants.Locks.MediaTypes, Cms.Core.Constants.Locks.MemberTypes);
                return Repository.GetAllContentTypeIds(aliases);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    internal class ContentTypeService : ContentTypeServiceBase<IContentTypeRepository, IContentType, IContentTypeService>, IContentTypeService
    {
        public ContentTypeService(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IContentService contentService)
            : base(provider, logger, eventMessagesFactory)
        {
            ContentService = contentService;
        }

        protected override IContentTypeService This => this;
        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Constants.Locks.ContentTypes };
        protected override int[] WriteLockIds { get; } = { Constants.Locks.ContentTree, Constants.Locks.ContentTypes };

        private IContentService ContentService { get; }

        protected override Guid ContainedObjectType => Constants.ObjectTypes.DocumentType;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                ContentService.DeleteOfType(typeId);
        }

        /// <summary>
        /// Gets all property type aliases accross content, media and member types.
        /// </summary>
        /// <returns>All property type aliases.</returns>
        /// <remarks>Beware! Works accross content, media and member types.</remarks>
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                // that one is special because it works accross content, media and member types
                uow.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);
                var repo = uow.CreateRepository<IContentTypeRepository>();
                return repo.GetAllPropertyTypeAliases();
            }
        }

        /// <summary>
        /// Gets all content type aliases accross content, media and member types.
        /// </summary>
        /// <param name="guids">Optional object types guid to restrict to content, and/or media, and/or member types.</param>
        /// <returns>All content type aliases.</returns>
        /// <remarks>Beware! Works accross content, media and member types.</remarks>
        public IEnumerable<string> GetAllContentTypeAliases(params Guid[] guids)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                // that one is special because it works accross content, media and member types
                uow.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);
                var repo = uow.CreateRepository<IContentTypeRepository>();
                return repo.GetAllContentTypeAliases(guids);
            }
        }

        /// <summary>
        /// Gets all content type id for aliases accross content, media and member types.
        /// </summary>
        /// <param name="aliases">Aliases to look for.</param>
        /// <returns>All content type ids.</returns>
        /// <remarks>Beware! Works accross content, media and member types.</remarks>
        public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                // that one is special because it works accross content, media and member types
                uow.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);
                var repo = uow.CreateRepository<IContentTypeRepository>();
                return repo.GetAllContentTypeIds(aliases);
            }
        }

    }
}

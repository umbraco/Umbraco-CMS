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
	    private IContentService _contentService;

        public ContentTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IContentService contentService)
            : base(provider, logger, eventMessagesFactory)
        {
            _contentService = contentService;
        }

        protected override IContentTypeService Instance => this;

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Constants.Locks.ContentTypes };
        protected override int[] WriteLockIds { get; } = { Constants.Locks.ContentTree, Constants.Locks.ContentTypes };

        // don't change or remove this, will need it later
        private IContentService ContentService => _contentService;
        //// handle circular dependencies
        //internal IContentService ContentService
        //{
        //    get
        //    {
        //        if (_contentService == null)
        //            throw new InvalidOperationException("ContentTypeService.ContentService has not been initialized.");
        //        return _contentService;
        //    }
        //    set { _contentService = value; }
        //}

        protected override Guid ContainedObjectType => Constants.ObjectTypes.DocumentTypeGuid;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                ContentService.DeleteContentOfType(typeId);
        }

        /// <summary>
        /// Gets all property type aliases accross content, media and member types.
        /// </summary>
        /// <returns>All property type aliases.</returns>
        /// <remarks>Beware! Works accross content, media and member types.</remarks>
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // that one is special because it works accross content, media and member types
                uow.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);
                var repo = uow.CreateRepository<IContentTypeRepository>();
                var aliases = repo.GetAllPropertyTypeAliases();
                uow.Complete();
                return aliases;
            }
        }

        /// <summary>
        /// Gets all content type aliases accross content, media and member types.
        /// </summary>
        /// <param name="guids">Optional object types guid to restrict to content, and/or media, and/or member types.</param>
        /// <returns>All property type aliases.</returns>
        /// <remarks>Beware! Works accross content, media and member types.</remarks>
        public IEnumerable<string> GetAllContentTypeAliases(params Guid[] guids)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // that one is special because it works accross content, media and member types
                uow.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);
                var repo = uow.CreateRepository<IContentTypeRepository>();
                var aliases = repo.GetAllContentTypeAliases(guids);
                uow.Complete();
                return aliases;
            }
        }
    }
}
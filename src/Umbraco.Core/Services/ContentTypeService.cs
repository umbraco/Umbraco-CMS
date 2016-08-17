using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    internal class ContentTypeService : ContentTypeServiceBase<IContentTypeRepository, IContentType, IContentTypeService>, IContentTypeService
    {
        public ContentTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IContentService contentService)
            : base(provider, logger, eventMessagesFactory)
        {
            ContentService = contentService;
        }

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Constants.Locks.ContentTypes };
        protected override int[] WriteLockIds { get; } = { Constants.Locks.ContentTree, Constants.Locks.ContentTypes };

        private IContentService ContentService { get; }

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

        /// <summary>
        /// Generates the complete (simplified) XML DTD.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        public string GetDtd()
        {
            var dtd = new StringBuilder();
            dtd.AppendLine("<!DOCTYPE root [ ");

            dtd.AppendLine(GetContentTypesDtd());
            dtd.AppendLine("]>");

            return dtd.ToString();
        }

        /// <summary>
        /// Generates the complete XML DTD without the root.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        public string GetContentTypesDtd()
        {
            var dtd = new StringBuilder();
            try
            {
                var strictSchemaBuilder = new StringBuilder();

                var contentTypes = GetAll(new int[0]);
                foreach (ContentType contentType in contentTypes)
                {
                    string safeAlias = contentType.Alias.ToSafeAlias();
                    if (safeAlias != null)
                    {
                        strictSchemaBuilder.AppendLine($"<!ELEMENT {safeAlias} ANY>");
                        strictSchemaBuilder.AppendLine($"<!ATTLIST {safeAlias} id ID #REQUIRED>");
                    }
                }

                // Only commit the strong schema to the container if we didn't generate an error building it
                dtd.Append(strictSchemaBuilder);
            }
            catch (Exception exception)
            {
                LogHelper.Error<ContentTypeService>("Error while trying to build DTD for Xml schema; is Umbraco installed correctly and the connection string configured?", exception);
            }
            return dtd.ToString();
        }

        protected override void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes)
        {
            var toUpdate = GetContentTypesForXmlUpdates(contentTypes).ToArray();
            if (toUpdate.Any() == false) return;

            var contentService = ContentService as ContentService;
            if (contentService != null)
            {
                contentService.RePublishAll(toUpdate.Select(x => x.Id).ToArray());
            }
            else
            {
                //this should never occur, the content service should always be typed but we'll check anyways.
                ContentService.RePublishAll();
            }
        }
    }
}
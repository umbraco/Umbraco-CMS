using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class MediaTypeService : ContentTypeServiceBase<IMediaTypeRepository, IMediaType, IMediaTypeService>, IMediaTypeService
    {
        public MediaTypeService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IMediaService mediaService,
            IMediaTypeRepository mediaTypeRepository, IAuditRepository auditRepository, IMediaTypeContainerRepository entityContainerRepository,
            IEntityRepository entityRepository)
            : base(provider, loggerFactory, eventMessagesFactory, mediaTypeRepository, auditRepository, entityContainerRepository, entityRepository)
        {
            MediaService = mediaService;
        }

        protected override IMediaTypeService This => this;

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Cms.Core.Constants.Locks.MediaTypes };
        protected override int[] WriteLockIds { get; } = { Cms.Core.Constants.Locks.MediaTree, Cms.Core.Constants.Locks.MediaTypes };

        private IMediaService MediaService { get; }

        protected override Guid ContainedObjectType => Cms.Core.Constants.ObjectTypes.MediaType;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                MediaService.DeleteMediaOfType(typeId);
        }
    }
}

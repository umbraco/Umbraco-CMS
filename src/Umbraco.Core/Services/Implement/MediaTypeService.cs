using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class MediaTypeService : ContentTypeServiceBase<IMediaTypeRepository, IMediaType, IMediaTypeService>, IMediaTypeService
    {
        public MediaTypeService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMediaService mediaService,
            IMediaTypeRepository mediaTypeRepository, IAuditRepository auditRepository, IMediaTypeContainerRepository entityContainerRepository,
            IEntityRepository entityRepository)
            : base(provider, logger, eventMessagesFactory, mediaTypeRepository, auditRepository, entityContainerRepository, entityRepository)
        {
            MediaService = mediaService;
        }

        protected override IMediaTypeService This => this;

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Constants.Locks.MediaTypes };
        protected override int[] WriteLockIds { get; } = { Constants.Locks.MediaTree, Constants.Locks.MediaTypes };

        private IMediaService MediaService { get; }

        protected override Guid ContainedObjectType => Constants.ObjectTypes.MediaType;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                MediaService.DeleteMediaOfType(typeId);
        }
    }
}

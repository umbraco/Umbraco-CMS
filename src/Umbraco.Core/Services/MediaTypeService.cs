using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal class MediaTypeService : ContentTypeServiceBase<IMediaTypeRepository, IMediaType, IMediaTypeService>, IMediaTypeService
    {
        public MediaTypeService(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMediaService mediaService)
            : base(provider, logger, eventMessagesFactory)
        {
            MediaService = mediaService;
        }

        protected override IMediaTypeService This => this;

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Constants.Locks.MediaTypes };
        protected override int[] WriteLockIds { get; } = { Constants.Locks.MediaTree, Constants.Locks.MediaTypes };

        private IMediaService MediaService { get; }

        protected override Guid ContainedObjectType => Constants.ObjectTypes.MediaTypeGuid;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                MediaService.DeleteMediaOfType(typeId);
        }
    }
}

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
        private IMediaService _mediaService;

        public MediaTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMediaService mediaService)
            : base(provider, logger, eventMessagesFactory)
        {
            _mediaService = mediaService;
        }

        protected override IMediaTypeService Instance => this;

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Constants.Locks.MediaTypes };
        protected override int[] WriteLockIds { get; } = { Constants.Locks.MediaTree, Constants.Locks.MediaTypes };

        // don't remove, will need it later
        private IMediaService MediaService => _mediaService;
        //// handle circular dependencies
        //internal IMediaService MediaService
        //{
        //    get
        //    {
        //        if (_mediaService == null)
        //            throw new InvalidOperationException("MediaTypeService.MediaService has not been initialized.");
        //        return _mediaService;
        //    }
        //    set { _mediaService = value; }
        //}

        protected override Guid ContainedObjectType => Constants.ObjectTypes.MediaTypeGuid;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                MediaService.DeleteMediaOfType(typeId);
        }
    }
}

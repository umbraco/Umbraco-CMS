using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal class MediaTypeService : ContentTypeServiceBase<IMediaTypeRepository, IMediaType, IMediaTypeService>, IMediaTypeService
    {
        public MediaTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMediaService mediaService)
            : base(provider, logger, eventMessagesFactory)
        {
            MediaService = mediaService;
        }

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

        protected override void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes)
        {
            var toUpdate = GetContentTypesForXmlUpdates(contentTypes).ToArray();
            if (toUpdate.Any() == false) return;

            var mediaService = MediaService as MediaService;
            mediaService?.RebuildXmlStructures(toUpdate.Select(x => x.Id).ToArray());
        }
    }
}

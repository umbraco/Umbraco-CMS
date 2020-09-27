using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting log history
    /// </summary>
    [PluginController("UmbracoApi")]
    public class LogController : UmbracoAuthorizedJsonController
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly IImageUrlGenerator _imageUrlGenerator;

        public LogController(
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IRuntimeState runtimeState,
            IMediaFileSystem mediaFileSystem,
            IShortStringHelper shortStringHelper,
            UmbracoMapper umbracoMapper,
            IImageUrlGenerator imageUrlGenerator,
            IPublishedUrlProvider publishedUrlProvider)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, shortStringHelper, umbracoMapper, publishedUrlProvider)
        {
            _mediaFileSystem = mediaFileSystem;
            _imageUrlGenerator = imageUrlGenerator;
        }

        [UmbracoApplicationAuthorize(Core.Constants.Applications.Content, Core.Constants.Applications.Media)]
        public PagedResult<AuditLog> GetPagedEntityLog(int id,
            int pageNumber = 1,
            int pageSize = 10,
            Direction orderDirection = Direction.Descending,
            DateTime? sinceDate = null)
        {
            if (pageSize <= 0 || pageNumber <= 0)
            {
                return new PagedResult<AuditLog>(0, pageNumber, pageSize);
            }

            long totalRecords;
            var result = Services.AuditService.GetPagedItemsByEntity(id, pageNumber - 1, pageSize, out totalRecords, orderDirection, sinceDate: sinceDate);
            var mapped = result.Select(item => Mapper.Map<AuditLog>(item));

            var page = new PagedResult<AuditLog>(totalRecords, pageNumber, pageSize)
            {
                Items = MapAvatarsAndNames(mapped)
            };

            return page;
        }

        public PagedResult<AuditLog> GetPagedCurrentUserLog(
            int pageNumber = 1,
            int pageSize = 10,
            Direction orderDirection = Direction.Descending,
            DateTime? sinceDate = null)
        {
            if (pageSize <= 0 || pageNumber <= 0)
            {
                return new PagedResult<AuditLog>(0, pageNumber, pageSize);
            }

            long totalRecords;
            var userId = Security.GetUserId().ResultOr(0);
            var result = Services.AuditService.GetPagedItemsByUser(userId, pageNumber - 1, pageSize, out totalRecords, orderDirection, sinceDate: sinceDate);
            var mapped = Mapper.MapEnumerable<IAuditItem, AuditLog>(result);
            return new PagedResult<AuditLog>(totalRecords, pageNumber, pageSize)
            {
                Items = MapAvatarsAndNames(mapped)
            };
        }

        private IEnumerable<AuditLog> MapAvatarsAndNames(IEnumerable<AuditLog> items)
        {
            var mappedItems = items.ToList();
            var userIds = mappedItems.Select(x => x.UserId).ToArray();
            var userAvatars = Services.UserService.GetUsersById(userIds)
                .ToDictionary(x => x.Id, x => x.GetUserAvatarUrls(AppCaches.RuntimeCache, _mediaFileSystem, _imageUrlGenerator));
            var userNames = Services.UserService.GetUsersById(userIds).ToDictionary(x => x.Id, x => x.Name);
            foreach (var item in mappedItems)
            {
                if (userAvatars.TryGetValue(item.UserId, out var avatars))
                {
                    item.UserAvatars = avatars;
                }
                if (userNames.TryGetValue(item.UserId, out var name))
                {
                    item.UserName = name;
                }


            }
            return mappedItems;
        }
    }
}

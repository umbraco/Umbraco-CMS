using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Mapping;
using Umbraco.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Authorization;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// The API controller used for getting log history
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    public class LogController : UmbracoAuthorizedJsonController
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly IAuditService _auditService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IUserService _userService;
        private readonly AppCaches _appCaches;
        private readonly ISqlContext _sqlContext;

        public LogController(
            IMediaFileSystem mediaFileSystem,
            IImageUrlGenerator imageUrlGenerator,
            IAuditService auditService,
            UmbracoMapper umbracoMapper,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IUserService userService,
            AppCaches appCaches,
            ISqlContext sqlContext)
         {
            _mediaFileSystem = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
            _imageUrlGenerator = imageUrlGenerator ?? throw new ArgumentNullException(nameof(imageUrlGenerator));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _appCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
            _sqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
         }

        [Authorize(Policy = AuthorizationPolicies.SectionAccessContentOrMedia)]
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
            var dateQuery = sinceDate.HasValue ? _sqlContext.Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;
            var result = _auditService.GetPagedItemsByEntity(id, pageNumber - 1, pageSize, out totalRecords, orderDirection, customFilter: dateQuery);
            var mapped = result.Select(item => _umbracoMapper.Map<AuditLog>(item));

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
            var dateQuery = sinceDate.HasValue ? _sqlContext.Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;
            var userId = _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0);
            var result = _auditService.GetPagedItemsByUser(userId, pageNumber - 1, pageSize, out totalRecords, orderDirection, customFilter:dateQuery);
            var mapped = _umbracoMapper.MapEnumerable<IAuditItem, AuditLog>(result);
            return new PagedResult<AuditLog>(totalRecords, pageNumber, pageSize)
            {
                Items = MapAvatarsAndNames(mapped)
            };
        }

        public IEnumerable<AuditLog> GetLog(AuditType logType, DateTime? sinceDate = null)
        {
            var result = _auditService.GetLogs(Enum<AuditType>.Parse(logType.ToString()), sinceDate);
            var mapped = _umbracoMapper.MapEnumerable<IAuditItem, AuditLog>(result);
            return mapped;
        }

        private IEnumerable<AuditLog> MapAvatarsAndNames(IEnumerable<AuditLog> items)
        {
            var mappedItems = items.ToList();
            var userIds = mappedItems.Select(x => x.UserId).ToArray();
            var userAvatars = _userService.GetUsersById(userIds)
                .ToDictionary(x => x.Id, x => x.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileSystem, _imageUrlGenerator));
            var userNames = _userService.GetUsersById(userIds).ToDictionary(x => x.Id, x => x.Name);
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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting log history
    /// </summary>
    [PluginController("UmbracoApi")]
    public class LogController : UmbracoAuthorizedJsonController
    {
        public PagedResult<AuditLog> GetPagedEntityLog(int id,
            int pageNumber = 1,
            int pageSize = 0,
            Direction orderDirection = Direction.Descending,
            DateTime? sinceDate = null)
        {
            long totalRecords;
            var dateQuery = sinceDate.HasValue ? Query<IAuditItem>.Builder.Where(x => x.CreateDate >= sinceDate) : null;
            var result = Services.AuditService.GetPagedItemsByEntity(id, pageNumber - 1, pageSize, out totalRecords, orderDirection, customFilter: dateQuery);
            var mapped = Mapper.Map<IEnumerable<AuditLog>>(result);
            
            var page = new PagedResult<AuditLog>(totalRecords, pageNumber, pageSize)
            {
                Items = MapAvatarsAndNames(mapped)
            };

            return page;
        }

        public PagedResult<AuditLog> GetPagedCurrentUserLog(
            int pageNumber = 1,
            int pageSize = 0,
            Direction orderDirection = Direction.Descending,
            DateTime? sinceDate = null)
        {
            long totalRecords;
            var dateQuery = sinceDate.HasValue ? Query<IAuditItem>.Builder.Where(x => x.CreateDate >= sinceDate) : null;
            var result = Services.AuditService.GetPagedItemsByUser(Security.GetUserId(), pageNumber - 1, pageSize, out totalRecords, orderDirection, customFilter:dateQuery);
            var mapped = Mapper.Map<IEnumerable<AuditLog>>(result);
            return new PagedResult<AuditLog>(totalRecords, pageNumber + 1, pageSize)
            {
                Items = MapAvatarsAndNames(mapped)
            };
        }

        [Obsolete("Use GetPagedLog instead")]
        public IEnumerable<AuditLog> GetEntityLog(int id)
        {
            long totalRecords;
            var result = Services.AuditService.GetPagedItemsByEntity(id, 1, int.MaxValue, out totalRecords);
            return Mapper.Map<IEnumerable<AuditLog>>(result);
        }

        //TODO: Move to CurrentUserController?
        [Obsolete("Use GetPagedCurrentUserLog instead")]
        public IEnumerable<AuditLog> GetCurrentUserLog(AuditType logType, DateTime? sinceDate)
        {
            long totalRecords;
            var dateQuery = sinceDate.HasValue ? Query<IAuditItem>.Builder.Where(x => x.CreateDate >= sinceDate) : null;
            var result = Services.AuditService.GetPagedItemsByUser(Security.GetUserId(), 1, int.MaxValue, out totalRecords, auditTypeFilter: new[] {logType},customFilter: dateQuery);
            return Mapper.Map<IEnumerable<AuditLog>>(result);
        }

        [Obsolete("Use GetPagedLog instead")]
        public IEnumerable<AuditLog> GetLog(AuditType logType, DateTime? sinceDate)
        {
            if (sinceDate == null)
                sinceDate = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0, 0));

            return Mapper.Map<IEnumerable<AuditLog>>(
                Log.Instance.GetLogItems(Enum<LogTypes>.Parse(logType.ToString()), sinceDate.Value));
        }

        private IEnumerable<AuditLog> MapAvatarsAndNames(IEnumerable<AuditLog> items)
        {
            var userIds = items.Select(x => x.UserId).ToArray();
            var users = Services.UserService.GetUsersById(userIds)
                .ToDictionary(x => x.Id, x => x.GetUserAvatarUrls(ApplicationContext.ApplicationCache.RuntimeCache));
            var userNames = Services.UserService.GetUsersById(userIds).ToDictionary(x => x.Id, x => x.Name);
            foreach (var item in items)
            {
                item.UserAvatars = users[item.UserId];
                item.UserName = userNames[item.UserId];
            }
            return items;
        }
    }
}

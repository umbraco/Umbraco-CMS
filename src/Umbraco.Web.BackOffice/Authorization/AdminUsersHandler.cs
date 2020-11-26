using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// if the users being edited is an admin then we must ensure that the current user is also an admin
    /// </summary>
    public class AdminUsersHandler : AuthorizationHandler<AdminUsersRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly IUserService _userService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly UserEditorAuthorizationHelper _userEditorAuthorizationHelper;

        public AdminUsersHandler(IHttpContextAccessor httpContextAcessor,
                IUserService userService,
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                UserEditorAuthorizationHelper userEditorAuthorizationHelper)
        {
            _httpContextAcessor = httpContextAcessor;
            _userService = userService;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _userEditorAuthorizationHelper = userEditorAuthorizationHelper;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminUsersRequirement requirement)
        {
            var queryString = _httpContextAcessor.HttpContext?.Request.Query[requirement.QueryStringName];
            if (!queryString.HasValue)
            {
                // must succeed this requirement since we cannot process it
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            int[] userIds;
            if (int.TryParse(queryString, out var userId))
            {
                userIds = new[] { userId };
            }
            else
            {
                var ids = _httpContextAcessor.HttpContext.Request.Query.Where(x => x.Key == requirement.QueryStringName).ToList();
                if (ids.Count == 0)
                {
                    // must succeed this requirement since we cannot process it
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
                userIds = ids.Select(x => x.Value.TryConvertTo<int>()).Where(x => x.Success).Select(x => x.Result).ToArray();
            }

            if (userIds.Length == 0)
            {
                // must succeed this requirement since we cannot process it
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var users = _userService.GetUsersById(userIds);
            var isAuth = users.All(user => _userEditorAuthorizationHelper.IsAuthorized(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, user, null, null, null) != false);

            if (isAuth)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}

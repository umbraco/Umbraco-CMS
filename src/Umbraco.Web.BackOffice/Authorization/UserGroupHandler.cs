// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Controllers;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorizes that the current user has access to the user group Id in the request
    /// </summary>
    public class UserGroupHandler : MustSatisfyRequirementAuthorizationHandler<UserGroupRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserGroupHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Accessor for the HTTP context of the current request.</param>
        /// <param name="userService">Service for user related operations.</param>
        /// <param name="contentService">Service for content related operations.</param>
        /// <param name="mediaService">Service for media related operations.</param>
        /// <param name="entityService">Service for entity related operations.</param>
        /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
        public UserGroupHandler(
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IContentService contentService,
            IMediaService mediaService,
            IEntityService entityService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _contentService = contentService;
            _mediaService = mediaService;
            _entityService = entityService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }

        /// <inheritdoc/>
        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, UserGroupRequirement requirement)
        {
            IUser currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser;

            IQueryCollection queryString = _httpContextAccessor.HttpContext?.Request.Query;
            if (queryString == null)
            {
                // Must succeed this requirement since we cannot process it.
                return Task.FromResult(true);
            }

            KeyValuePair<string, StringValues>[] ids = queryString.Where(x => x.Key == requirement.QueryStringName).ToArray();
            if (ids.Length == 0)
            {
                // Must succeed this requirement since we cannot process it.
                return Task.FromResult(true);
            }

            var intIds = ids
                .Select(x => x.Value.ToString())
                .Select(x => x.TryConvertTo<int>()).Where(x => x.Success).Select(x => x.Result).ToArray();

            var authHelper = new UserGroupEditorAuthorizationHelper(
                _userService,
                _contentService,
                _mediaService,
                _entityService);

            Attempt<string> isAuth = authHelper.AuthorizeGroupAccess(currentUser, intIds);

            return Task.FromResult(isAuth.Success);
        }
    }
}

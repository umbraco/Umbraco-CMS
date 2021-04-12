using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security
{
    public class PublicAccessChecker : IPublicAccessChecker
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPublicAccessService _publicAccessService;
        private readonly IContentService _contentService;

        public PublicAccessChecker(IHttpContextAccessor httpContextAccessor, IPublicAccessService publicAccessService, IContentService contentService)
        {
            _httpContextAccessor = httpContextAccessor;
            _publicAccessService = publicAccessService;
            _contentService = contentService;
        }

        public async Task<PublicAccessStatus> HasMemberAccessToContentAsync(int publishedContentId)
        {
            HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();
            IMemberManager memberManager = httpContext.RequestServices.GetRequiredService<IMemberManager>();
            if (httpContext.User.Identity == null || !httpContext.User.Identity.IsAuthenticated)
            {
                return PublicAccessStatus.NotLoggedIn;
            }
            MemberIdentityUser currentMember = await memberManager.GetUserAsync(httpContext.User);
            if (currentMember == null)
            {
                return PublicAccessStatus.NotLoggedIn;
            }

            var username = currentMember.UserName;
            System.Collections.Generic.IList<string> userRoles = await memberManager.GetRolesAsync(currentMember);

            if (!_publicAccessService.HasAccess(publishedContentId, _contentService, username, userRoles))
            {
                return PublicAccessStatus.AccessDenied;
            }

            if (!currentMember.IsApproved)
            {
                return PublicAccessStatus.NotApproved;
            }

            if (currentMember.IsLockedOut)
            {
                return PublicAccessStatus.LockedOut;
            }

            return PublicAccessStatus.AccessAccepted;
        }
    }
}

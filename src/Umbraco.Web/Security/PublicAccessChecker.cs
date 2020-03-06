using System;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.Security
{
    public class PublicAccessChecker : IPublicAccessChecker
    {
        //TODO: This is lazy to avoid circular dependency. We don't care right now because all membership is going to be changed.
        private readonly Lazy<MembershipHelper> _membershipHelper;
        private readonly IPublicAccessService _publicAccessService;
        private readonly IContentService _contentService;

        public PublicAccessChecker(Lazy<MembershipHelper> membershipHelper, IPublicAccessService publicAccessService, IContentService contentService)
        {
            _membershipHelper = membershipHelper;
            _publicAccessService = publicAccessService;
            _contentService = contentService;
        }

        public PublicAccessStatus HasMemberAccessToContent(int publishedContentId)
        {
            var membershipHelper = _membershipHelper.Value;

            if (membershipHelper.IsLoggedIn() == false)
            {
                return PublicAccessStatus.NotLoggedIn;
            }

            var username = membershipHelper.CurrentUserName;
            var userRoles = membershipHelper.GetCurrentUserRoles();

            if (_publicAccessService.HasAccess(publishedContentId, _contentService, username, userRoles) == false)
            {
                return PublicAccessStatus.AccessDenied;
            }

            var member = membershipHelper.GetCurrentMember();

            if (member.HasProperty(Constants.Conventions.Member.IsApproved) == false)
            {
                return PublicAccessStatus.NotApproved;
            }

            if (member.HasProperty(Constants.Conventions.Member.IsLockedOut) &&
                member.Value<bool>(Constants.Conventions.Member.IsApproved))
            {
                return PublicAccessStatus.LockedOut;
            }

            return PublicAccessStatus.AccessAccepted;
        }
    }
}

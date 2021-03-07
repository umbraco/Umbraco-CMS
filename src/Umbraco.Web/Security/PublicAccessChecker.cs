using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.Security
{
    public class PublicAccessChecker : IPublicAccessChecker
    {
        private readonly IPublicAccessService _publicAccessService;
        private readonly IContentService _contentService;
        private readonly IPublishedValueFallback _publishedValueFallback;

        public PublicAccessChecker(IPublicAccessService publicAccessService, IContentService contentService, IPublishedValueFallback publishedValueFallback)
        {
            _publicAccessService = publicAccessService;
            _contentService = contentService;
            _publishedValueFallback = publishedValueFallback;
        }

        public PublicAccessStatus HasMemberAccessToContent(int publishedContentId)
        {
            //var membershipHelper = new MembershipHelper();
            //if (membershipHelper.IsLoggedIn() == false)
            //{
            //    return PublicAccessStatus.NotLoggedIn;
            //}

            //var username = membershipHelper.CurrentUserName;
            //var userRoles = membershipHelper.GetCurrentUserRoles();

            if (_publicAccessService.HasAccess(publishedContentId, _contentService, null, null) == false)
            {
                return PublicAccessStatus.AccessDenied;
            }

            //var member = membershipHelper.GetCurrentMember();

            //if (member.HasProperty(Constants.Conventions.Member.IsApproved) == false)
            //{
            //    return PublicAccessStatus.NotApproved;
            //}

            //if (member.HasProperty(Constants.Conventions.Member.IsLockedOut) &&
            //    member.Value<bool>(_publishedValueFallback, Constants.Conventions.Member.IsApproved))
            //{
            //    return PublicAccessStatus.LockedOut;
            //}

            return PublicAccessStatus.AccessAccepted;
        }
    }
}

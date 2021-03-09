using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Web.Common.Security
{
    /// <summary>
    /// Ensures that the <see cref="IUmbracoWebsiteSecurity"/> is populated on a front-end request
    /// </summary>
    internal sealed class UmbracoWebsiteSecurityFactory : INotificationHandler<UmbracoRoutedRequest>
    {
        private readonly IUmbracoWebsiteSecurityAccessor _umbracoWebsiteSecurityAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IShortStringHelper _shortStringHelper;

        public UmbracoWebsiteSecurityFactory(
            IUmbracoWebsiteSecurityAccessor umbracoWebsiteSecurityAccessor,
            IHttpContextAccessor httpContextAccessor,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IShortStringHelper shortStringHelper)
        {
            _umbracoWebsiteSecurityAccessor = umbracoWebsiteSecurityAccessor;
            _httpContextAccessor = httpContextAccessor;
            _memberService = memberService;
            _memberTypeService = memberTypeService;
            _shortStringHelper = shortStringHelper;
        }

        public void Handle(UmbracoRoutedRequest notification)
        {
            if (_umbracoWebsiteSecurityAccessor.WebsiteSecurity is null)
            {
                _umbracoWebsiteSecurityAccessor.WebsiteSecurity = new UmbracoWebsiteSecurity(
                    _httpContextAccessor,
                    _memberService,
                    _memberTypeService,
                    _shortStringHelper);
            }
        }
    }
}

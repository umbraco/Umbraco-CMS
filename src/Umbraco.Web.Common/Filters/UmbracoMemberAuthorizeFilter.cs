using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Web.Common.Filters
{

    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public class UmbracoMemberAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IUmbracoWebsiteSecurity _websiteSecurity;

        public UmbracoMemberAuthorizeFilter(IUmbracoWebsiteSecurity websiteSecurity)
        {
            _websiteSecurity = websiteSecurity;
        }

        /// <summary>
        /// Comma delimited list of allowed member types
        /// </summary>
        public string AllowType { get; private set;}

        /// <summary>
        /// Comma delimited list of allowed member groups
        /// </summary>
        public string AllowGroup { get; private set;}

        /// <summary>
        /// Comma delimited list of allowed members
        /// </summary>
        public string AllowMembers { get; private set; }

        private UmbracoMemberAuthorizeFilter(string allowType, string allowGroup, string allowMembers)
        {
            AllowType = allowType;
            AllowGroup = allowGroup;
            AllowMembers = allowMembers;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!IsAuthorized())
            {
                context.HttpContext.SetReasonPhrase("Resource restricted: either member is not logged on or is not of a permitted type or group.");
                context.Result = new ForbidResult();
            }
        }

        private bool IsAuthorized()
        {
            if (AllowMembers.IsNullOrWhiteSpace())
            {
                AllowMembers = string.Empty;
            }

            if (AllowGroup.IsNullOrWhiteSpace())
            {
                AllowGroup = string.Empty;
            }

            if (AllowType.IsNullOrWhiteSpace())
            {
                AllowType = string.Empty;
            }

            var members = new List<int>();
            foreach (var s in AllowMembers.Split(','))
            {
                if (int.TryParse(s, out var id))
                {
                    members.Add(id);
                }
            }

            return _websiteSecurity.IsMemberAuthorized(AllowType.Split(','), AllowGroup.Split(','), members);
        }
    }
}

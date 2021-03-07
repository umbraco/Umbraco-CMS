using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Umbraco.Extensions;
using Umbraco.Web.Security;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Mvc
{
    //TODO: update for identity
    /// <summary>
    /// Attribute for attributing controller actions to restrict them
    /// to just authenticated members, and optionally of a particular type and/or group
    /// </summary>
    public sealed class MemberAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Comma delimited list of allowed member types
        /// </summary>
        public string AllowType { get; set; }

        /// <summary>
        /// Comma delimited list of allowed member groups
        /// </summary>
        public string AllowGroup { get; set; }

        /// <summary>
        /// Comma delimited list of allowed members
        /// </summary>
        public string AllowMembers { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (AllowMembers.IsNullOrWhiteSpace())
            {
                AllowMembers = "";
            }

            if (AllowGroup.IsNullOrWhiteSpace())
            {
                AllowGroup = "";
            }

            if (AllowType.IsNullOrWhiteSpace())
            {
                AllowType = "";
            }

            var members = new List<int>();
            foreach (var s in AllowMembers.Split(','))
            {
                if (int.TryParse(s, out var id))
                {
                    members.Add(id);
                }
            }

            MembershipHelper helper = Current.MembershipHelper;
            return helper.IsMemberAuthorized(AllowType.Split(','), AllowGroup.Split(','), members).Result;
        }

        /// <summary>
        /// Override method to throw exception instead of returning a 401 result
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) => throw new HttpException(403, "Resource restricted: either member is not logged on or is not of a permitted type or group.");

    }
}

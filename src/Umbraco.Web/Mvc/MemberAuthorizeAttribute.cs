using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using umbraco.cms.businesslogic.member;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Attribute for attributing controller actions to restrict them
    /// to just authenticated members, and optionally of a particular type and/or group
    /// </summary>
    public class MemberAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Flag for whether to allow all site visitors or just authenticated members
        /// </summary>
        /// <remarks>
        /// This is the same as applying the [AllowAnonymous] attribute
        /// </remarks>
        public bool AllowAll { get; set; }

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
            // Allow by default
            var allowAction = true;

            // If not set to allow all, need to check current loggined in member
            if (!AllowAll)
            {
                // Get member details
                var member = Member.GetCurrentMember();
                if (member == null)
                {
                    // If not logged on, not allowed
                    allowAction = false;
                }
                else
                {
                    // If types defined, check member is of one of those types
                    if (!string.IsNullOrEmpty(AllowType))
                    {
                        // Allow only if member's type is in list
                        allowAction = AllowType.ToLower().Split(',').Contains(member.ContentType.Alias.ToLower());
                    }

                    // If groups defined, check member is of one of those groups
                    if (allowAction && !string.IsNullOrEmpty(AllowGroup))
                    {
                        // Allow only if member's type is in list
                        var groups = System.Web.Security.Roles.GetRolesForUser(member.LoginName);
                        allowAction = groups.Select(s => s.ToLower()).Intersect(AllowGroup.ToLower().Split(',')).Any();
                    }

                    // If specific members defined, check member is of one of those
                    if (allowAction && !string.IsNullOrEmpty(AllowMembers))
                    {
                        // Allow only if member's type is in list
                        allowAction = AllowMembers.ToLower().Split(',').Contains(member.Id.ToString());
                    }
                }
            }
            return allowAction;
        }

        /// <summary>
        /// Override method to throw exception instead of returning a 401 result
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            throw new HttpException(403, "Resource restricted: either member is not logged on or is not of a permitted type or group.");
        }

    }
}

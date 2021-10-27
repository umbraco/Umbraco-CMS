using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Security;
using Umbraco.Core.Composing;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.WebApi
{
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

        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (AllowMembers.IsNullOrWhiteSpace())
                AllowMembers = "";
            if (AllowGroup.IsNullOrWhiteSpace())
                AllowGroup = "";
            if (AllowType.IsNullOrWhiteSpace())
                AllowType = "";

            var members = new List<int>();
            foreach (var s in AllowMembers.Split(Constants.CharArrays.Comma))
            {
                if (int.TryParse(s, out var id))
                {
                    members.Add(id);
                }
            }

            var helper = Current.Factory.GetInstance<MembershipHelper>();
            return helper.IsMemberAuthorized(AllowType.Split(Constants.CharArrays.Comma), AllowGroup.Split(Constants.CharArrays.Comma), members);
        }

    }
}

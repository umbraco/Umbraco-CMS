using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Filters
{

    /// <summary>
    /// Ensures authorization is successful for a front-end member
    /// </summary>
    public class UmbracoMemberAuthorizeFilter : IAsyncAuthorizationFilter
    {
        public UmbracoMemberAuthorizeFilter()
        {
        }

        public  UmbracoMemberAuthorizeFilter(string allowType, string allowGroup, string allowMembers)
        {
            AllowType = allowType;
            AllowGroup = allowGroup;
            AllowMembers = allowMembers;
        }

        /// <summary>
        /// Comma delimited list of allowed member types
        /// </summary>
        public string AllowType { get; private set; }

        /// <summary>
        /// Comma delimited list of allowed member groups
        /// </summary>
        public string AllowGroup { get; private set; }

        /// <summary>
        /// Comma delimited list of allowed members
        /// </summary>
        public string AllowMembers { get; private set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            IMemberManager memberManager = context.HttpContext.RequestServices.GetRequiredService<IMemberManager>();

            if (!await IsAuthorizedAsync(memberManager))
            {
                context.HttpContext.SetReasonPhrase("Resource restricted: either member is not logged on or is not of a permitted type or group.");
                context.Result = new ForbidResult();
            }
        }

        private async Task<bool> IsAuthorizedAsync(IMemberManager memberManager)
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
            foreach (var s in AllowMembers.Split(Core.Constants.CharArrays.Comma))
            {
                if (int.TryParse(s, out var id))
                {
                    members.Add(id);
                }
            }

            return await memberManager.IsMemberAuthorizedAsync(AllowType.Split(Core.Constants.CharArrays.Comma), AllowGroup.Split(Core.Constants.CharArrays.Comma), members);
        }
    }
}

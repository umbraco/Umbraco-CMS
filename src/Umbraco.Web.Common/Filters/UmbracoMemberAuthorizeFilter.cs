﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Extensions;

namespace Umbraco.Web.Common.Filters
{

    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public class UmbracoMemberAuthorizeFilter : IAuthorizationFilter
    {
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


        private UmbracoMemberAuthorizeFilter(
            string allowType, string allowGroup, string allowMembers)
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
                AllowMembers = "";
            if (AllowGroup.IsNullOrWhiteSpace())
                AllowGroup = "";
            if (AllowType.IsNullOrWhiteSpace())
                AllowType = "";

            var members = new List<int>();
            foreach (var s in AllowMembers.Split(','))
            {
                if (int.TryParse(s, out var id))
                {
                    members.Add(id);
                }
            }

            return false;// TODO reintroduce when members are implemented: _memberHelper.IsMemberAuthorized(AllowType.Split(','), AllowGroup.Split(','), members);
        }
    }
}

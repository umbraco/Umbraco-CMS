﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Filters
{
    /// <summary>
    /// This inspects the result of the action that returns a collection of content and removes
    /// any item that the current user doesn't have access to
    /// </summary>
    internal sealed class FilterAllowedOutgoingContentAttribute : TypeFilterAttribute
    {

        internal FilterAllowedOutgoingContentAttribute(Type outgoingType)
            : this(outgoingType, null, ActionBrowse.ActionLetter)
        {

        }

        internal FilterAllowedOutgoingContentAttribute(Type outgoingType, char permissionToCheck)
            : this(outgoingType, null, permissionToCheck)
        {
        }

        internal FilterAllowedOutgoingContentAttribute(Type outgoingType, string propertyName)
            : this(outgoingType, propertyName, ActionBrowse.ActionLetter)
        {
        }

        internal FilterAllowedOutgoingContentAttribute(Type outgoingType, IUserService userService, IEntityService entityService)
            : this(outgoingType, null, ActionBrowse.ActionLetter)
        {

        }

        public FilterAllowedOutgoingContentAttribute(Type outgoingType, string propertyName, char permissionToCheck)
            : base(typeof(FilterAllowedOutgoingContentFilter))
        {
            Arguments = new object[]
            {
                outgoingType, propertyName, permissionToCheck
            };
        }
    }
    internal sealed class FilterAllowedOutgoingContentFilter : FilterAllowedOutgoingMediaFilter
    {
        private readonly char _permissionToCheck;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;
        private readonly AppCaches _appCaches;

        public FilterAllowedOutgoingContentFilter(Type outgoingType, string propertyName, char permissionToCheck,  IUserService userService, IEntityService entityService, AppCaches appCaches, IBackOfficeSecurityAccessor backofficeSecurityAccessor)
            : base(entityService, backofficeSecurityAccessor, appCaches, outgoingType, propertyName)
        {
            _permissionToCheck = permissionToCheck;
            _userService = userService;
            _entityService = entityService;
            _appCaches = appCaches;
        }

        protected override void FilterItems(IUser user, IList items)
        {
            base.FilterItems(user, items);

            FilterBasedOnPermissions(items, user);
        }

        protected override int[] GetUserStartNodes(IUser user)
        {
            return user.CalculateContentStartNodeIds(_entityService, _appCaches);
        }

        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinContent; }
        }

        internal void FilterBasedOnPermissions(IList items, IUser user)
        {
            var length = items.Count;

            if (length > 0)
            {
                var ids = new List<int>();
                for (var i = 0; i < length; i++)
                {
                    ids.Add(((dynamic)items[i]).Id);
                }
                //get all the permissions for these nodes in one call
                var permissions = _userService.GetPermissions(user, ids.ToArray());
                var toRemove = new List<dynamic>();
                foreach (dynamic item in items)
                {
                    //get the combined permission set across all user groups for this node
                    //we're in the world of dynamics here so we need to cast
                    var nodePermission = ((IEnumerable<string>)permissions.GetAllPermissions(item.Id)).ToArray();

                    //if the permission being checked doesn't exist then remove the item
                    if (nodePermission.Contains(_permissionToCheck.ToString(CultureInfo.InvariantCulture)) == false)
                    {
                        toRemove.Add(item);
                    }
                }
                foreach (var item in toRemove)
                {
                    items.Remove(item);
                }
            }
        }

    }
}

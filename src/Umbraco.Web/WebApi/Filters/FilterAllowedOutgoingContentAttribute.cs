using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// This inspects the result of the action that returns a collection of content and removes 
    /// any item that the current user doesn't have access to
    /// </summary>
    internal sealed class FilterAllowedOutgoingContentAttribute : FilterAllowedOutgoingMediaAttribute
    {
        private readonly char _permissionToCheck;

        public FilterAllowedOutgoingContentAttribute(Type outgoingType) 
            : base(outgoingType)
        {
            _permissionToCheck = ActionBrowse.Instance.Letter;
        }

        public FilterAllowedOutgoingContentAttribute(Type outgoingType, char permissionToCheck)
            : base(outgoingType)
        {
            _permissionToCheck = permissionToCheck;
        }

        public FilterAllowedOutgoingContentAttribute(Type outgoingType, string propertyName)
            : base(outgoingType, propertyName)
        {
        }

        protected override void FilterItems(IUser user, List<dynamic> items)
        {
            base.FilterItems(user, items);

            FilterBasedOnPermissions(items, user, ApplicationContext.Current.Services.UserService);
        }
        
        internal void FilterBasedOnPermissions(List<dynamic> items, IUser user, IUserService userService)
        {
            var length = items.Count;
            var ids = new List<int>();
            for (var i = 0; i < length; i++)
            {
                ids.Add(items[i].Id);
            }
            //get all the permissions for these nodes in one call
            var permissions = userService.GetPermissions(user, ids.ToArray()).ToArray();
            var toRemove = new List<dynamic>();
            foreach(var item in items)
            {
                var nodePermission = permissions.Where(x => x.EntityId == item.Id).ToArray();
                //if there are no permissions for this id, then remove the item
                if (nodePermission.Any() == false)
                {
                    toRemove.Add(item);                    
                }
                else
                {
                    foreach (var n in nodePermission)
                    {
                        //if the permission being checked doesn't exist then remove the item
                        if (n.AssignedPermissions.Contains(_permissionToCheck.ToString(CultureInfo.InvariantCulture)) == false)
                        {
                            toRemove.Add(item);
                        }
                    }
                }
            }
            foreach (var item in toRemove)
            {
                items.Remove(item);
            }
        }

    }
}
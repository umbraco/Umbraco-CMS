using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Filters;
using Umbraco.Web.Models.ContentEditing;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{

    //TODO: Verify that this works!!

    /// <summary>
    /// This inspects the result of the action that returns a collection of content and removes 
    /// any item that the current user doesn't have access to
    /// </summary>
    internal sealed class FilterAllowedOutgoingContentAttribute : ActionFilterAttribute
    {
        private readonly string _propertyName;
        private readonly char _permissionToCheck;

        public FilterAllowedOutgoingContentAttribute()
        {
            _permissionToCheck = ActionBrowse.Instance.Letter;
        }

        public FilterAllowedOutgoingContentAttribute(char permissionToCheck)
        {
            _permissionToCheck = permissionToCheck;
        }

        public FilterAllowedOutgoingContentAttribute(string propertyName)
            : this()
        {
            _propertyName = propertyName;
        }

        /// <summary>
        /// Returns true so that other filters can execute along with this one
        /// </summary>
        public override bool AllowMultiple
        {
            get { return true; }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var user = UmbracoContext.Current.Security.CurrentUser;
            if (user == null)
            {
                base.OnActionExecuted(actionExecutedContext);
                return;
            }

            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            if (objectContent != null)
            {
                var collection = GetValueFromResponse(objectContent);

                if (collection != null)
                {
                    var items = Enumerable.ToList(collection);
                    var length = items.Count;
                    var ids = new List<int>();                    
                    for (var i = 0; i < length; i++)
                    {
                        ids.Add(items[i].Id);
                    }
                    //get all the permissions for these nodes in one call
                    var permissions = ApplicationContext.Current.Services.UserService.GetPermissions(user, ids.ToArray()).ToArray();
                    for (var i = 0; i < length; i++)
                    {
                        var nodePermission = permissions.Where(x => x.EntityId == items[i].Id).ToArray();
                        foreach (var n in nodePermission)
                        {
                            if (n.AssignedPermissions.Contains(_permissionToCheck.ToString(CultureInfo.InvariantCulture)) == false)
                            {
                                items.RemoveAt(i);
                                length--;
                            }
                        }
                    }

                    //set the return value
                    SetValueForResponse(objectContent, items);
                }
            }

            base.OnActionExecuted(actionExecutedContext);
        }

        private void SetValueForResponse(ObjectContent objectContent, dynamic newVal)
        {
            var t = objectContent.Value.GetType();
            
            if (objectContent.Value is IEnumerable<ContentItemBasic>)
            {
                //objectContent.Value = DynamicCast(newVal, t);
                objectContent.Value = newVal;
            }
            else if (_propertyName.IsNullOrWhiteSpace() == false)
            {
                //try to get the enumerable collection from a property on the result object using reflection
                var property = objectContent.Value.GetType().GetProperty(_propertyName);
                if (property != null)
                {                  
                    property.SetValue(objectContent.Value, newVal);                    
                }
            }
            
        }

        private dynamic GetValueFromResponse(ObjectContent objectContent)
        {
            if (objectContent.Value is IEnumerable<ContentItemBasic>)
            {
                return objectContent.Value;
            }

            if (_propertyName.IsNullOrWhiteSpace() == false)
            {
                //try to get the enumerable collection from a property on the result object using reflection
                var property = objectContent.Value.GetType().GetProperty(_propertyName);
                if (property != null)
                {
                    var result = property.GetValue(objectContent.Value);
                    if (result is IEnumerable<ContentItemBasic>)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
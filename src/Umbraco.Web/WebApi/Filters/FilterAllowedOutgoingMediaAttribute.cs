using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Trees;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// This inspects the result of the action that returns a collection of content and removes
    /// any item that the current user doesn't have access to
    /// </summary>
    internal class FilterAllowedOutgoingMediaAttribute : ActionFilterAttribute
    {
        private readonly Type _outgoingType;
        private readonly string _propertyName;

        public FilterAllowedOutgoingMediaAttribute(Type outgoingType)
        {
            _outgoingType = outgoingType;
        }

        public FilterAllowedOutgoingMediaAttribute(Type outgoingType, string propertyName)
            : this(outgoingType)
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

        protected virtual int[] GetUserStartNodes(IUser user)
        {
            return user.CalculateMediaStartNodeIds(ApplicationContext.Current.Services.EntityService);
        }

        protected virtual int RecycleBinId
        {
            get { return Constants.System.RecycleBinMedia; }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response == null) return;

            var user = UmbracoContext.Current.Security.CurrentUser;
            if (user == null) return;

            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            if (objectContent != null)
            {
                var collection = GetValueFromResponse(objectContent);

                if (collection != null)
                {
                    var items = Enumerable.ToList(collection);

                    FilterItems(user, items);

                    //set the return value
                    SetValueForResponse(objectContent, items);
                }
            }

            base.OnActionExecuted(actionExecutedContext);
        }

        protected virtual void FilterItems(IUser user, IList items)
        {

            Guid? dataTypeId = Guid.TryParse(HttpContext.Current.Request.QueryString.Get(TreeQueryStringParameters.DataTypeId), out var temp) ? (Guid?)temp : null;

            if (dataTypeId.HasValue == false) return;

            var ignoreUserStartNodes = ApplicationContext.Current.Services.DataTypeService.IsDataTypeIgnoringUserStartNodes(dataTypeId.Value);

            if (ignoreUserStartNodes == false)
            {
                FilterBasedOnStartNode(items, user);
            }
            else
            {
                FilterOutPossibleSensitiveData(items);
            }

        }

        /// <summary>
        /// Removes all properties of the items except the umbracoFile.
        /// </summary>
        /// <param name="items"></param>
        private void FilterOutPossibleSensitiveData(IList items)
        {
            foreach (dynamic item in items)
            {

                if (item.Properties != null)
                {
                    if (item.Properties is IList properties)
                        // Iterate reverse, because we removed from the same list, such that the ordering of indexes not
                        // changes doing the iteration
                        for (var i = properties.Count - 1; i >= 0; i--)
                        {
                            dynamic property = properties[i];
                            if (property.Alias != null && property.Alias is string)
                            {
                                var alias = property.Alias as string;
                                if (string.Equals(alias, Constants.Conventions.Media.File) == false)
                                {
                                    properties.RemoveAt(i);
                                }
                            }
                        }
                }
            }
        }

        internal void FilterBasedOnStartNode(IList items, IUser user)
        {
            var toRemove = new List<dynamic>();
            foreach (dynamic item in items)
            {
                var hasPathAccess = (item != null && UserExtensions.HasPathAccess(item.Path, GetUserStartNodes(user), RecycleBinId));
                if (hasPathAccess == false)
                {
                    toRemove.Add(item);
                }
            }

            foreach (var item in toRemove)
            {
                items.Remove(item);
            }
        }

        private void SetValueForResponse(ObjectContent objectContent, dynamic newVal)
        {
            if (TypeHelper.IsTypeAssignableFrom(_outgoingType, objectContent.Value.GetType()))
            {
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

        internal dynamic GetValueFromResponse(ObjectContent objectContent)
        {
            if (TypeHelper.IsTypeAssignableFrom(_outgoingType, objectContent.Value.GetType()))
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
                    if (result != null && TypeHelper.IsTypeAssignableFrom(_outgoingType, result.GetType()))
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}

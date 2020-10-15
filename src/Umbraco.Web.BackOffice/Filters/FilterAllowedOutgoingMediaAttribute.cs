using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Security;


namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// This inspects the result of the action that returns a collection of content and removes
    /// any item that the current user doesn't have access to
    /// </summary>
    internal class FilterAllowedOutgoingMediaAttribute : TypeFilterAttribute
    {
        public FilterAllowedOutgoingMediaAttribute(Type outgoingType, string propertyName = null)
            : base(typeof(FilterAllowedOutgoingMediaFilter))
        {
            Arguments = new object[]
            {
                outgoingType, propertyName
            };
        }
    }
    internal class FilterAllowedOutgoingMediaFilter : IActionFilter
    {
        private readonly Type _outgoingType;
        private readonly IEntityService _entityService;
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly string _propertyName;

        public FilterAllowedOutgoingMediaFilter(IEntityService entityService, IBackofficeSecurityAccessor backofficeSecurityAccessor, Type outgoingType, string propertyName)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));

            _propertyName = propertyName;
            _outgoingType = outgoingType;
        }

        protected virtual int[] GetUserStartNodes(IUser user)
        {
            return user.CalculateMediaStartNodeIds(_entityService);
        }

        protected virtual int RecycleBinId => Constants.System.RecycleBinMedia;

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result == null) return;

            var user = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser;
            if (user == null) return;

            var objectContent = context.Result as ObjectResult;
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
        }

        protected virtual void FilterItems(IUser user, IList items)
        {
            FilterBasedOnStartNode(items, user);
        }

        internal void FilterBasedOnStartNode(IList items, IUser user)
        {
            var toRemove = new List<dynamic>();
            foreach (dynamic item in items)
            {
                var hasPathAccess = (item != null && ContentPermissionsHelper.HasPathAccess(item.Path, GetUserStartNodes(user), RecycleBinId));
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

        private void SetValueForResponse(ObjectResult objectContent, dynamic newVal)
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

        internal dynamic GetValueFromResponse(ObjectResult objectContent)
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


        public void OnActionExecuting(ActionExecutingContext context)
        {

        }
    }
}

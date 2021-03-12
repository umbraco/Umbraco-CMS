using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Auth filter to check if the current user has access to the content item
    /// </summary>
    /// <remarks>
    /// Since media doesn't have permissions, this simply checks start node access
    /// </remarks>
    internal sealed class EnsureUserPermissionForMediaAttribute : ActionFilterAttribute
    {
        private readonly int? _nodeId;
        private readonly string _paramName;

        public enum DictionarySource
        {
            ActionArguments,
            RequestForm,
            RequestQueryString
        }

        /// <summary>
        /// This constructor will only be able to test the start node access
        /// </summary>
        public EnsureUserPermissionForMediaAttribute(int nodeId)
        {
            _nodeId = nodeId;
        }

        public EnsureUserPermissionForMediaAttribute(string paramName)
        {
            if (paramName == null) throw new ArgumentNullException(nameof(paramName));
            if (string.IsNullOrEmpty(paramName)) throw new ArgumentException("Value can't be empty.", nameof(paramName));

            _paramName = paramName;
        }

        // TODO: v8 guess this is not used anymore, source is ignored?!
        public EnsureUserPermissionForMediaAttribute(string paramName, DictionarySource source)
        {
            if (paramName == null) throw new ArgumentNullException(nameof(paramName));
            if (string.IsNullOrEmpty(paramName)) throw new ArgumentException("Value can't be empty.", nameof(paramName));

            _paramName = paramName;
        }

        public override bool AllowMultiple => true;

        private int GetNodeIdFromParameter(object parameterValue)
        {
            if (parameterValue is int)
            {
                return (int) parameterValue;
            }

            var guidId = Guid.Empty;
            if (parameterValue is Guid)
            {
                guidId = (Guid)parameterValue;
            }
            else if (parameterValue is GuidUdi)
            {
                guidId = ((GuidUdi) parameterValue).Guid;
            }

            if (guidId != Guid.Empty)
            {
                var found =  Current.Services.EntityService.GetId(guidId, UmbracoObjectTypes.Media);
                if (found)
                    return found.Result;
            }

            throw new InvalidOperationException("The id type: " + parameterValue.GetType() + " is not a supported id");
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (Current.UmbracoContext.Security.CurrentUser == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            int nodeId;
            if (_nodeId.HasValue == false)
            {
                var parts = _paramName.Split(Constants.CharArrays.Period, StringSplitOptions.RemoveEmptyEntries);

                if (actionContext.ActionArguments[parts[0]] == null)
                {
                    throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                }

                if (parts.Length == 1)
                {
                    nodeId = GetNodeIdFromParameter(actionContext.ActionArguments[parts[0]]);
                }
                else
                {
                    //now we need to see if we can get the property of whatever object it is
                    var pType = actionContext.ActionArguments[parts[0]].GetType();
                    var prop = pType.GetProperty(parts[1]);
                    if (prop == null)
                    {
                        throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                    }
                    nodeId = GetNodeIdFromParameter(prop.GetValue(actionContext.ActionArguments[parts[0]]));
                }
            }
            else
            {
                nodeId = _nodeId.Value;
            }

            if (MediaController.CheckPermissions(
                actionContext.Request.Properties,
                Current.UmbracoContext.Security.CurrentUser,
                Current.Services.MediaService,
                Current.Services.EntityService,
                Current.AppCaches,
                nodeId))
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }
        }
    }
}

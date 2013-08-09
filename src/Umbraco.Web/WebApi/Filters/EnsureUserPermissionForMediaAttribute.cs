using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using umbraco.BusinessLogic.Actions;

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

        /// <summary>
        /// This constructor will only be able to test the start node access
        /// </summary>
        public EnsureUserPermissionForMediaAttribute(int nodeId)
        {
            _nodeId = nodeId;
        }

        public EnsureUserPermissionForMediaAttribute(string paramName)
        {
            Mandate.ParameterNotNullOrEmpty(paramName, "paramName");
            _paramName = paramName;            
        }
       
        public override bool AllowMultiple
        {
            get { return true; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (UmbracoContext.Current.Security.CurrentUser == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            int nodeId;
            if (_nodeId.HasValue == false)
            {
                if (actionContext.ActionArguments[_paramName] == null)
                {
                    throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                }

                nodeId = (int) actionContext.ActionArguments[_paramName];
            }
            else
            {
                nodeId = _nodeId.Value;
            }

            if (CheckPermissions(
                actionContext.Request.Properties,
                UmbracoContext.Current.Security.CurrentUser, 
                ApplicationContext.Current.Services.MediaService, nodeId))
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }
            
        }

        internal bool CheckPermissions(IDictionary<string, object> storage , IUser user, IMediaService mediaService, int nodeId)
        {
            var contentItem = mediaService.GetById(nodeId);
            if (contentItem == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            //put the content item into storage so it can be retreived 
            // in the controller (saves a lookup)
            storage.Add(typeof(IMedia).ToString(), contentItem);

            var hasPathAccess = user.HasPathAccess(contentItem);

            return hasPathAccess;
        }

    }
}
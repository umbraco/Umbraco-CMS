using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

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
        private int? _nodeId;
        private readonly IUser _user;
        private readonly IMediaService _mediaService;
        private IMediaService MediaService
        {
            get { return _mediaService ?? ApplicationContext.Current.Services.MediaService; }
        }
        private IUser User
        {
            get { return _user ?? UmbracoContext.Current.Security.CurrentUser; }
        }

        private readonly string _paramName;

        /// <summary>
        /// used for unit testing
        /// </summary>
        /// <param name="user"></param>
        /// <param name="mediaService"></param>
        /// <param name="nodeId"></param>
        internal EnsureUserPermissionForMediaAttribute(IUser user, IMediaService mediaService, int nodeId)
        {
            _user = user;
            _mediaService = mediaService;
            _nodeId = nodeId;
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
            _paramName = paramName;         
        }        

        public override bool AllowMultiple
        {
            get { return true; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (User == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            if (_nodeId.HasValue == false)
            {
                if (actionContext.ActionArguments[_paramName] == null)
                {
                    throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                }

                _nodeId = (int)actionContext.ActionArguments[_paramName];
            }

            var mediaItem = MediaService.GetById(_nodeId.Value);
            if (mediaItem == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            var hasPathAccess = User.HasPathAccess(mediaItem);

            if (hasPathAccess == false)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            base.OnActionExecuting(actionContext);
        }

    }
}
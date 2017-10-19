using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Authorizes that the current user has access to the user group Id in the request
    /// </summary>
    internal class UserGroupAuthorizationAttribute : AuthorizeAttribute
    {
        private readonly string _paramName;
        private readonly UmbracoContext _umbracoContext;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="umbracoContext"></param>
        public UserGroupAuthorizationAttribute(string paramName, UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _paramName = paramName;
            _umbracoContext = umbracoContext;
        }

        public UserGroupAuthorizationAttribute(string paramName)
        {
            _paramName = paramName;
        }

        private UmbracoContext GetUmbracoContext()
        {
            return _umbracoContext ?? UmbracoContext.Current;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var umbCtx = GetUmbracoContext();
            var currentUser = umbCtx.Security.CurrentUser;
            
            var queryString = actionContext.Request.GetQueryNameValuePairs();

            var ids = queryString.Where(x => x.Key == _paramName).ToArray();
            if (ids.Length == 0)
                return base.IsAuthorized(actionContext);

            var intIds = ids.Select(x => x.Value.TryConvertTo<int>()).Where(x => x.Success).Select(x => x.Result).ToArray();
            var authHelper = new UserGroupEditorAuthorizationHelper(
                umbCtx.Application.Services.UserService,
                umbCtx.Application.Services.ContentService,
                umbCtx.Application.Services.MediaService,
                umbCtx.Application.Services.EntityService);
            return authHelper.AuthorizeGroupAccess(currentUser, intIds);         
        }
    }
}
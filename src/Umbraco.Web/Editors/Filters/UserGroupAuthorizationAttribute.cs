using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Authorizes that the current user has access to the user group Id in the request
    /// </summary>
    internal class UserGroupAuthorizationAttribute : AuthorizeAttribute
    {
        private readonly string _paramName;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="umbracoContextAccessor"></param>
        public UserGroupAuthorizationAttribute(string paramName, IUmbracoContextAccessor umbracoContextAccessor)
        {
            if (umbracoContextAccessor == null) throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _paramName = paramName;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public UserGroupAuthorizationAttribute(string paramName)
        {
            _paramName = paramName;
        }

        private UmbracoContext GetUmbracoContext()
        {
            return _umbracoContextAccessor?.UmbracoContext ?? Composing.Current.UmbracoContext;
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
                Current.Services.UserService,
                Current.Services.ContentService,
                Current.Services.MediaService,
                Current.Services.EntityService,
                Current.AppCaches);
            return authHelper.AuthorizeGroupAccess(currentUser, intIds);
        }
    }
}

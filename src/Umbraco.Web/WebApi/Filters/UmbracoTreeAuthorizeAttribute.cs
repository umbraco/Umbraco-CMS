using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the current user has access to the application for which the specified tree(s) belongs
    /// </summary>
    /// <remarks>
    /// This would allow a tree to be moved between sections
    /// </remarks>
    public sealed class UmbracoTreeAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly string[] _treeAliases;

        /// <summary>
        /// Constructor to set authorization to be based on a tree alias for which application security will be applied
        /// </summary>
        /// <param name="treeAliases">
        /// If the user has access to the application that the treeAlias is specified in, they will be authorized.
        /// Multiple trees may be specified.
        /// </param>
        public UmbracoTreeAuthorizeAttribute(params string[] treeAliases)
        {
            _treeAliases = treeAliases;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (Enable == false)
            {
                return true;
            }

            var apps = _treeAliases.Select(x => ApplicationContext.Current.Services.ApplicationTreeService
                .GetByAlias(x))
                .WhereNotNull()
                .Select(x => x.ApplicationAlias)
                .Distinct()
                .ToArray();

            var result = false;
            if (UmbracoContext.Current.Security.CurrentUser != null)
            {
                result = apps.Any(app => UmbracoContext.Current.Security.UserHasAppAccess(
                       app, UmbracoContext.Current.Security.CurrentUser));

                // Special case for recycle bin: exists in content and media, but needs to request details from 
                // the list view data type configuration via a controller restricted to users that have access to the developer application.
                // So if the request is for this, allow it even if the user does not have access to the developer application.
                if (!result)
                {
                    if (actionContext.Request.RequestUri.Segments.Length == 6 &&
                        actionContext.Request.RequestUri.Segments[4].ToLowerInvariant() == "datatype/" &&
                        actionContext.Request.RequestUri.Segments[5].ToLowerInvariant() == "getbyid" &&
                        !string.IsNullOrEmpty(actionContext.Request.RequestUri.Query))
                    {
                        var querystring = HttpUtility.ParseQueryString(actionContext.Request.RequestUri.Query.ToLowerInvariant());
                        int requestedId;
                        if (querystring.ContainsKey("id") && int.TryParse(querystring["id"], out requestedId))
                        {
                            string viewingApp = null;
                            switch (requestedId)
                            {
                                case Constants.System.DefaultContentListViewDataTypeId:
                                    viewingApp = "content";
                                    break;
                                case Constants.System.DefaultMediaListViewDataTypeId:
                                    viewingApp = "media";
                                    break;
                            }

                            if (!string.IsNullOrEmpty(viewingApp))
                            {
                                result = UmbracoContext.Current.Security.UserHasAppAccess(viewingApp, UmbracoContext.Current.Security.CurrentUser);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
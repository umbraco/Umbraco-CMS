using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates the list of action buttons allowed for this user - Publish, Send to publish, save, unpublish returned as the button's 'letter'
    /// </summary>
    internal class ActionButtonsResolver
    {
        public ActionButtonsResolver(IUserService userService, IContentService contentService)
        {
            UserService = userService;
            ContentService = contentService;
        }

        private IUserService UserService { get; }
        private IContentService ContentService { get; }

        public IEnumerable<string> Resolve(IContent source)
        {
            //cannot check permissions without a context
            if (Current.UmbracoContext == null)
                return Enumerable.Empty<string>();

            string path;
            if (source.HasIdentity)
                path = source.Path;
            else
            {
                var parent = ContentService.GetById(source.ParentId);
                path = parent == null ? "-1" : parent.Path;
            }

            // TODO: This is certainly not ideal usage here - perhaps the best way to deal with this in the future is
            // with the IUmbracoContextAccessor. In the meantime, if used outside of a web app this will throw a null
            // reference exception :(
            return UserService.GetPermissionsForPath(Current.UmbracoContext.Security.CurrentUser, path).GetAllPermissions();
        }
    }
}

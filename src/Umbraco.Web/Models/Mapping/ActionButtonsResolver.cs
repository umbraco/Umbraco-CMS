using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    //TODO: This is horribly inneficient
    /// Creates the list of action buttons allowed for this user - Publish, Send to publish, save, unpublish returned as the button's 'letter'
    /// </summary>
    internal class ActionButtonsResolver
    {
        private readonly Lazy<IUserService> _userService;

        public ActionButtonsResolver(Lazy<IUserService> userService)
        {
            _userService = userService;
        }

        public IEnumerable<char> Resolve(IContent source)
        {
            if (UmbracoContext.Current == null)
            {
                //cannot check permissions without a context
                return Enumerable.Empty<char>();
            }
            var svc = _userService.Value;

            var permissions = svc.GetPermissions(
                    //TODO: This is certainly not ideal usage here - perhaps the best way to deal with this in the future is
                    // with the IUmbracoContextAccessor. In the meantime, if used outside of a web app this will throw a null
                    // refrence exception :(
                    UmbracoContext.Current.Security.CurrentUser,
                    // Here we need to do a special check since this could be new content, in which case we need to get the permissions
                    // from the parent, not the existing one otherwise permissions would be coming from the root since Id is 0.
                    source.HasIdentity ? source.Id : source.ParentId)
                .FirstOrDefault();

            return permissions == null
                ? Enumerable.Empty<char>()
                : permissions.AssignedPermissions.Where(x => x.Length == 1).Select(x => x.ToUpperInvariant()[0]);
        }
    }
}
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Security
{
    internal class CurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public CurrentUserAccessor(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <inheritdoc/>
        public IUser TryGetCurrentUser()
        {
            return _umbracoContextAccessor.UmbracoContext?.Security?.CurrentUser;
        }
    }
}

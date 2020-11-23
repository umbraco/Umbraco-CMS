using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Umbraco.Core.Models.Security;
using Umbraco.Core.Security;

namespace Umbraco.Web.Website.Security
{
    public class UmbracoWebsiteSecurity : IUmbracoWebsiteSecurity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UmbracoWebsiteSecurity(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public Task<RegisterMemberStatus> RegisterMemberAsync(RegisterModel model, bool logMemberIn = true)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<UpdateMemberProfileResult> UpdateMemberProfileAsync(ProfileModel model)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsLoggedIn()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User != null && httpContext.User.Identity.IsAuthenticated;
        }

        /// <inheritdoc/>
        public Task<bool> LoginAsync(string username, string password)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task LogOutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <inheritdoc/>
        public bool IsMemberAuthorized(IEnumerable<string> allowTypes = null, IEnumerable<string> allowGroups = null, IEnumerable<int> allowMembers = null)
        {
            throw new System.NotImplementedException();
        }
    }
}

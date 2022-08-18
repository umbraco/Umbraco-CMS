using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.OAuth;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessAuthorizedServices)]
    public class AuthorizedServiceController : BackOfficeNotificationsController
    {
        internal const string State = "abc123"; // TODO: This needs to be a random string.

        private readonly AuthorizedServiceSettings _authorizedServiceSettings;
        private readonly ITokenStorage _tokenStorage;

        public AuthorizedServiceController(
            IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings,
            ITokenStorage tokenStorage)
        {
            _authorizedServiceSettings = authorizedServiceSettings.CurrentValue;
            _tokenStorage = tokenStorage;
        }

        public AuthorizedServiceDisplay? GetByAlias(string alias)
        {
            ServiceDetail? serviceDetail = _authorizedServiceSettings.Services.SingleOrDefault(x => x.Alias == alias);
            if (serviceDetail == null)
            {
                return null;
            }

            bool tokenExists = _tokenStorage.GetToken(alias) != null;

            string? authorizationUrl = null;
            if (!tokenExists)
            {
                authorizationUrl = BuildAuthorizationUrl(serviceDetail);
            }

            return new AuthorizedServiceDisplay
            {
                DisplayName = serviceDetail.DisplayName,
                IsAuthorized = tokenExists,
                AuthorizationUrl = authorizationUrl,
                SampleRequest = serviceDetail.SampleRequest
            };
        }

        private string BuildAuthorizationUrl(ServiceDetail serviceDetail)
        {
            var url = new StringBuilder();
            url.Append(serviceDetail.IdentityHost);
            url.Append(serviceDetail.RequestIdentityPath);
            url.Append("?client_id=").Append(serviceDetail.ClientId);

            if (serviceDetail.RequestIdentityRequiresRedirectUri)
            {
                var redirectUri = HttpContext.GetAuthorizedServiceRedirectUri();
                url.Append("&redirect_uri=").Append(redirectUri);
            }

            url.Append("&scope=").Append(serviceDetail.Scopes);

            url.Append("&state=").Append(serviceDetail.Alias + "|" + State);

            return url.ToString();
        }
    }
}


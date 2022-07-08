using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.OAuth;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    public class AuthorizedServiceResponseController : UmbracoApiController
    {
        private readonly IAuthorizedServiceCaller _authorizedServiceCaller;

        public AuthorizedServiceResponseController(
            IAuthorizedServiceCaller authorizedServiceCaller)
        {
            _authorizedServiceCaller = authorizedServiceCaller;
        }

        public async Task<IActionResult> HandleIdentityResponse(string code, string state)
        {
            var stateParts = state.Split('|');
            if (stateParts.Length != 2 && stateParts[1] != AuthorizedServiceController.State)
            {
                throw new InvalidOperationException("State doesn't match.");
            }

            var serviceAlias = stateParts[0];

            AuthorizationResult result = await _authorizedServiceCaller.AuthorizeServiceAsync(serviceAlias, new Dictionary<string, string> { { "code", code } });
            if (result.Success)
            {
                return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceAlias}");
            }

            throw new InvalidOperationException("Failed to obtain access token");
        }
    }
}


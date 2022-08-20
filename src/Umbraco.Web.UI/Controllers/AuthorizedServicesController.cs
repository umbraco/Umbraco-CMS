using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.OAuth;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.UI.Models;

namespace Umbraco.Cms.Web.UI.Controllers
{
    public class AuthorizedServicesController : UmbracoApiController
    {
        private readonly IAuthorizedServiceCaller _authorizedServiceCaller;

        public AuthorizedServicesController(
            IAuthorizedServiceCaller authorizedServiceCaller)
        {
            _authorizedServiceCaller = authorizedServiceCaller;
        }

        public async Task<IActionResult> GetUmbracoContributorsFromGitHub()
        {
            List<GitHubContributor> response = await _authorizedServiceCaller.SendRequestAsync<List<GitHubContributor>>(
                "github",
                "/repos/Umbraco/Umbraco-CMS/contributors",
                HttpMethod.Get);
            return Content(string.Join(", ", response.Select(x => x.Login)));
        }

        public async Task<IActionResult> GetContactsFromHubspot()
        {
            HubspotContactResponse response = await _authorizedServiceCaller.SendRequestAsync<HubspotContactResponse>(
                "hubspot",
                "/crm/v3/objects/contacts?limit=10&archived=false",
                HttpMethod.Get);
            return Content(string.Join(", ", response.Results.Select(x => x.Properties.FirstName + " " + x.Properties.LastName)));
        }
    }
}


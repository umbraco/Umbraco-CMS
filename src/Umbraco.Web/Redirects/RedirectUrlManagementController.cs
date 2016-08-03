using System.IO;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Redirects
{
    public class RedirectUrlManagementController : UmbracoAuthorizedApiController
    {
        //add paging
        [HttpGet]
        public RedirectUrlSearchResult SearchRedirectUrls(string searchTerm, int page = 0, int pageSize = 10)
        {
            page = page - 1;
            var searchResult = new RedirectUrlSearchResult { UrlTrackerDisabled = UmbracoConfig.For.UmbracoSettings().WebRouting.DisableRedirectUrlTracking };
            var redirectUrlService = Services.RedirectUrlService;
            long resultCount = 0L;
            // need endpoint for search functionality
            // by url, by domain ? it's the url that you want to find them by, that's what you see..

            var redirects = redirectUrlService.GetAllRedirectUrls(page, pageSize, out resultCount);
            searchResult.SearchResults = redirects;
            searchResult.TotalCount = resultCount;
            searchResult.CurrentPage = page + 1;
            //hmm how many results 'could there be ?
            searchResult.PageCount = ((int)resultCount + pageSize - 1) / pageSize;

            searchResult.HasSearchResults = resultCount > 0;
            searchResult.HasExactMatch = (resultCount == 1);
            return searchResult;

        }

        [HttpGet]
        public HttpResponseMessage GetPublishedUrl(int id)
        {
            var publishedUrl = "#";
            if (id > 0)
            {
                publishedUrl = Umbraco.Url(id);
            }

            return new HttpResponseMessage { Content = new StringContent(publishedUrl, Encoding.UTF8, "text/html") };

        }

        [HttpPost]
        public HttpResponseMessage DeleteRedirectUrl(int id)
        {

            var redirectUrlService = Services.RedirectUrlService;
            // has the redirect already been deleted ?
            //var redirectUrl = redirectUrlService.GetById(redirectUrl.Id);
            //if (redirectUrl== null)
            //{
            //    return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            //}
            redirectUrlService.Delete(id);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpResponseMessage ToggleUrlTracker(bool disable)
        {
            var configFilePath = HttpContext.Current.Server.MapPath("~/config/umbracoSettings.config");

            if (File.Exists(configFilePath))
            {
                var umbracoConfig = new XmlDocument {PreserveWhitespace = true};
                umbracoConfig.Load(configFilePath);

                var webRoutingElement = umbracoConfig.SelectSingleNode("//web.routing") as XmlElement;
                if (webRoutingElement != null)
                {
                    // note: this adds the attribute if it does not exist
                    webRoutingElement.SetAttribute("disableRedirectUrlTracking", disable.ToString().ToLowerInvariant());
                    umbracoConfig.Save(configFilePath);
                }


                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Redirects
{
    public class RedirectUrlManagementController : UmbracoAuthorizedApiController
    {
        //add paging
        [HttpGet]
        public RedirectUrlSearchResult SearchRedirectUrls(string searchTerm, int page = 0)
        {

            int pageSize = 20;
            var searchResult = new RedirectUrlSearchResult();
            var redirectUrlService = Services.RedirectUrlService;
            long resultCount = 0L;
            // need endpoint for search functionality
            // by url, by domain ? it's the url that you want to find them by, that's what you see..

            var redirects = redirectUrlService.GetAllRedirectUrls(page, 20, out resultCount);
            searchResult.SearchResults = redirects;
            searchResult.TotalCount = resultCount;
            searchResult.CurrentPage = page;
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


    }
}
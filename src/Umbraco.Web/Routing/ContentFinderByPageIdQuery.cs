namespace Umbraco.Web.Routing
{
    /// <summary>
    /// This looks up a document by checking for the umbPageId of a request/query string
    /// </summary>
    /// <remarks>
    /// This is used by library.RenderTemplate and also some of the macro rendering functionality like in
    /// macroResultWrapper.aspx
    /// </remarks>
    public class ContentFinderByPageIdQuery : IContentFinder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentFinderByPageIdQuery(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool TryFindContent(IPublishedRequest frequest)
        {
            int pageId;
            if (int.TryParse(_httpContextAccessor.HttpContext.Request["umbPageID"], out pageId))
            {
                var doc = frequest.UmbracoContext.Content.GetById(pageId);

                if (doc != null)
                {
                    frequest.PublishedContent = doc;
                    return true;
                }
            }
            return false;
        }
    }
}

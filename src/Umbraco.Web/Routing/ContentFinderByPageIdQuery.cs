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
        public bool TryFindContent(PublishedRequest frequest)
        {
            int pageId;
            if (int.TryParse(frequest.UmbracoContext.HttpContext.Request["umbPageID"], out pageId))
            {
                var doc = frequest.UmbracoContext.ContentCache.GetById(pageId);

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

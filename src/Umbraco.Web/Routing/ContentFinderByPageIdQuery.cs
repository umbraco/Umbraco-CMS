namespace Umbraco.Web.Routing
{
	/// <summary>
	/// This looks up a document by checking for the umbPageId of a request/query string
	/// </summary>
	/// <remarks>
	/// This is used by library.RenderTemplate and also some of the macro rendering functionality like in
	/// insertMacro.aspx and macroResultWrapper.aspx
	/// </remarks>
    public class ContentFinderByPageIdQuery : IContentFinder
	{
		public bool TryFindContent(PublishedContentRequest docRequest)
		{
			int pageId;
			if (int.TryParse(docRequest.RoutingContext.UmbracoContext.HttpContext.Request["umbPageID"], out pageId))
			{
				var doc = docRequest.RoutingContext.UmbracoContext.ContentCache.GetById(pageId);

				if (doc != null)
				{
					docRequest.PublishedContent = doc;
					return true;
				}
			}
			return false;
		}
	}
}
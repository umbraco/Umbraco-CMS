using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs the legacy 404 logic.
	/// </summary>
	public class ContentFinderByLegacy404 : IContentFinder
	{
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="pcr">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindContent(PublishedContentRequest pcr)
		{
			LogHelper.Debug<ContentFinderByLegacy404>("Looking for a page to handle 404.");

            // TODO - replace the whole logic and stop calling into library!
			var error404 = global::umbraco.library.GetCurrentNotFoundPageId();
			var id = int.Parse(error404);

			IPublishedContent content = null;

			if (id > 0)
			{
				LogHelper.Debug<ContentFinderByLegacy404>("Got id={0}.", () => id);

				content = pcr.RoutingContext.UmbracoContext.ContentCache.GetById(id);

			    LogHelper.Debug<ContentFinderByLegacy404>(content == null
			        ? "Could not find content with that id."
			        : "Found corresponding content.");
			}
			else
			{
				LogHelper.Debug<ContentFinderByLegacy404>("Got nothing.");
			}

			pcr.PublishedContent = content;
            pcr.SetIs404();
			return content != null;
		}
	}
}

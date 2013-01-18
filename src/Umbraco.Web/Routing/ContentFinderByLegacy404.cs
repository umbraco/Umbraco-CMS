using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs the legacy 404 logic.
	/// </summary>
	/// <remarks>Should be used as a last chance finder.</remarks>
	internal class ContentFinderByLegacy404 : IContentFinder
	{
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="pcr">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindDocument(PublishedContentRequest pcr)
		{
			LogHelper.Debug<ContentFinderByLegacy404>("Looking for a page to handler 404.");

			var error404 = global::umbraco.library.GetCurrentNotFoundPageId();
			var id = int.Parse(error404);

			IPublishedContent content = null;

			if (id > 0)
			{
				LogHelper.Debug<ContentFinderByLegacy404>("Got id={1}.", () => id);

				content = pcr.RoutingContext.PublishedContentStore.GetDocumentById(
						pcr.RoutingContext.UmbracoContext,
						id);

				if (content == null)
					LogHelper.Debug<ContentFinderByLegacy404>("Could not find content with that id.");
				else
					LogHelper.Debug<ContentFinderByLegacy404>("Found corresponding content.");
			}
			else
			{
				LogHelper.Debug<ContentFinderByLegacy404>("Got nothing.");
			}

			pcr.PublishedContent = content;
			return content != null;
		}
	}
}

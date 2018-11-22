using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs legacy <c>INotFoundHandler</c> in "last chance" situation.
	/// </summary>
    public class ContentLastChanceFinderByNotFoundHandlers : IContentFinder
    {
		// notes
		//
		// at the moment we load the legacy INotFoundHandler
		// excluding those that have been replaced by proper finders,
		// and run them.

		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindContent(PublishedContentRequest docRequest)
        {
			HandlePageNotFound(docRequest);
            return docRequest.HasPublishedContent;
        }

		#region Copied over and adapted from presentation.requestHandler

	    private static void HandlePageNotFound(PublishedContentRequest docRequest)
        {			
			var url = NotFoundHandlerHelper.GetLegacyUrlForNotFoundHandlers();
            LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Running for legacy url='{0}'.", () => url);

	        var handler = NotFoundHandlerHelper.GetNotFoundLastChanceHandler();
            var handlerName = handler.GetType().FullName;
            LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Handler '{0}'.", () => handlerName);

            var finder = NotFoundHandlerHelper.SubsituteFinder(handler);
            if (finder != null)
            {
                var finderName = finder.GetType().FullName;
                LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Replace handler '{0}' by new finder '{1}'.", () => handlerName, () => finderName);

                // can't find a document => exit
                if (finder.TryFindContent(docRequest) == false)
                    return;

                // found a document => we're done

                // in theory an IContentFinder can return true yet set no document
                // but none of the substitued finders (see SubstituteFinder) do it.

                // do NOT set docRequest.PublishedContent again here
                // as it would clear any template that the finder might have set

                LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Finder '{0}' found node with id={1}.", () => finderName, () => docRequest.PublishedContent.Id);
                if (docRequest.Is404)
                    LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Finder '{0}' set status to 404.", () => finderName);

                LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Handler '{0}' found valid node with id={1}.", () => handlerName, () => docRequest.PublishedContent.Id);
                return;
            }

            // else it's a legacy handler, run

            // can't find a document => exit
            if (handler.Execute(url) == false || handler.redirectID <= 0)
                return;

            // found a document ID => ensure it's a valid document
            var redirectId = handler.redirectID;
            docRequest.PublishedContent = docRequest.RoutingContext.UmbracoContext.ContentCache.GetById(redirectId);

            if (docRequest.HasPublishedContent == false)
            {
                // the handler said it could handle the url, and returned a content ID
                // yet that content ID is invalid... exit.

                LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Handler '{0}' found node with id={1} which is not valid.", () => handlerName, () => redirectId);
                return;
            }

            // found a valid document => return

            LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Handler '{0}' found valid node with id={1}.", () => handlerName, () => redirectId);

            if (docRequest.RoutingContext.UmbracoContext.HttpContext.Response.StatusCode == 404)
            {
                LogHelper.Debug<ContentLastChanceFinderByNotFoundHandlers>("Handler '{0}' set status code to 404.", () => handlerName);
                docRequest.Is404 = true;
            }
        }

		#endregion
	}
}
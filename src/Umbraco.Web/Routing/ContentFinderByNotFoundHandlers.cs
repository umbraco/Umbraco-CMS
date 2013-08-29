using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs legacy <c>INotFoundHandler</c>.
	/// </summary>
    public class ContentFinderByNotFoundHandlers : IContentFinder
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
            LogHelper.Debug<ContentFinderByNotFoundHandlers>("Running for legacy url='{0}'.", () => url);

            foreach (var handler in NotFoundHandlerHelper.GetNotFoundHandlers())
            {
                var handlerName = handler.GetType().FullName;
                LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}'.", () => handlerName);

                var finder = NotFoundHandlerHelper.SubsituteFinder(handler);
                if (finder != null)
                {
                    var finderName = finder.GetType().FullName;
                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Replace handler '{0}' by new finder '{1}'.", () => handlerName, () => finderName);

                    // can't find a document => continue with other handlers
                    if (finder.TryFindContent(docRequest) == false)
                        continue;

                    // found a document => break, don't run other handlers, we're done

                    // in theory an IContentFinder can return true yet set no document
                    // but none of the substitued finders (see SubstituteFinder) do it.

                    // do NOT set docRequest.PublishedContent again here
                    // as it would clear any template that the finder might have set

                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Finder '{0}' found node with id={1}.", () => finderName, () => docRequest.PublishedContent.Id);
                    if (docRequest.Is404)
                        LogHelper.Debug<ContentFinderByNotFoundHandlers>("Finder '{0}' set status to 404.", () => finderName);

                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' found valid node with id={1}.", () => handlerName, () => docRequest.PublishedContent.Id);
                    break;
                }

                // else it's a legacy handler: run

                // can't find a document => continue with other handlers
                if (handler.Execute(url) == false || handler.redirectID <= 0)
                    continue;

                // found a document ID => ensure it's a valid document
                var redirectId = handler.redirectID;
                docRequest.PublishedContent = docRequest.RoutingContext.UmbracoContext.ContentCache.GetById(redirectId);

                if (docRequest.HasPublishedContent == false)
                {
                    // the handler said it could handle the url, and returned a content ID
                    // yet that content ID is invalid... should we run the other handlers?
                    // I don't think so, not here, let the "last chance" finder take care.
                    // so, break.

                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' found node with id={1} which is not valid.", () => handlerName, () => redirectId);
                    break;
                }

                // found a valid document => break, don't run other handlers, we're done

                LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' found valid node with id={1}.", () => handlerName, () => redirectId);

                if (docRequest.RoutingContext.UmbracoContext.HttpContext.Response.StatusCode == 404)
                {
                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' set status code to 404.", () => handlerName);
                    docRequest.Is404 = true;
                }

                break;
            }
        }

		#endregion
	}
}
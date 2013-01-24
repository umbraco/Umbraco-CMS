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

		void HandlePageNotFound(PublishedContentRequest docRequest)
        {			
			var url = NotFoundHandlerHelper.GetLegacyUrlForNotFoundHandlers();
            LogHelper.Debug<ContentFinderByNotFoundHandlers>("Running for legacy url='{0}'.", () => url);

            foreach (var handler in GetNotFoundHandlers())
            {
                IContentFinder finder = null;
                var handlerName = handler.GetType().FullName;

                LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}'.", () => handlerName);

                // replace with our own implementation
                if (handler is global::umbraco.SearchForAlias)
                    finder = new ContentFinderByUrlAlias();
                else if (handler is global::umbraco.SearchForProfile)
                    finder = new ContentFinderByProfile();
                else if (handler is global::umbraco.SearchForTemplate)
                    finder = new ContentFinderByNiceUrlAndTemplate();
                else if (handler is global::umbraco.handle404)
                    finder = new ContentFinderByLegacy404();

                if (finder != null)
                {
                    var finderName = finder.GetType().FullName;
                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Replace handler '{0}' by new finder '{1}'.", () => handlerName, () => finderName);
                    if (finder.TryFindContent(docRequest))
                    {
                        // do NOT set docRequest.PublishedContent again here as 
                        // it would clear any template that the finder might have set
                        LogHelper.Debug<ContentFinderByNotFoundHandlers>("Finder '{0}' found node with id={1}.", () => finderName, () => docRequest.PublishedContent.Id);
                        if (docRequest.Is404)
                            LogHelper.Debug<ContentFinderByNotFoundHandlers>("Finder '{0}' set status to 404.", () => finderName);

                        // if we found a document, break, don't look at more handler -- we're done
                        break;
                    }

                    // if we did not find a document, continue, look at other handlers
                    continue;
                }

                // else it's a legacy handler, run

				if (handler.Execute(url) && handler.redirectID > 0)
				{
				    var redirectId = handler.redirectID;
                    docRequest.PublishedContent = docRequest.RoutingContext.UmbracoContext.ContentCache.GetById(redirectId);

                    if (!docRequest.HasPublishedContent)
                    {
                        LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' found node with id={1} which is not valid.", () => handlerName, () => redirectId);
                        break;
                    }

                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' found valid node with id={1}.", () => handlerName, () => redirectId);

                    if (docRequest.RoutingContext.UmbracoContext.HttpContext.Response.StatusCode == 404)
                    {
                        LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' set status code to 404.", () => handlerName);
                        docRequest.Is404 = true;
                    }

                    //// check for caching
                    //if (handler.CacheUrl)
                    //{
                    //    if (url.StartsWith("/"))
                    //        url = "/" + url;

                    //    var cacheKey = (currentDomain == null ? "" : currentDomain.Name) + url;
                    //    var culture = currentDomain == null ? null : currentDomain.Language.CultureAlias;
                    //    SetCache(cacheKey, new CacheEntry(handler.redirectID.ToString(), culture));

                    //    HttpContext.Current.Trace.Write("NotFoundHandler",
                    //        string.Format("Added to cache '{0}', {1}.", url, handler.redirectID));
                    //}

                    // if we found a document, break, don't look at more handler -- we're done
                    break;
                }

                // if we did not find a document, continue, look at other handlers
            }
        }

        IEnumerable<INotFoundHandler> GetNotFoundHandlers()
        {
            // instanciate new handlers
            // using definition cache

            var handlers = new List<INotFoundHandler>();

            foreach (var type in NotFoundHandlerHelper.CustomHandlerTypes)
            {
                try
                {
                    var handler = Activator.CreateInstance(type) as INotFoundHandler;
                    if (handler != null)
                        handlers.Add(handler);
                }
                catch (Exception e)
                {
					LogHelper.Error<ContentFinderByNotFoundHandlers>(string.Format("Error instanciating handler {0}, ignoring.", type.FullName), e);                         
                }
            }

            return handlers;
		}

		#endregion
	}
}
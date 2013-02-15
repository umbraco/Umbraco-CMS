using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.IO;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs legacy <c>INotFoundHandler</c>.
	/// </summary>
    internal class ContentFinderByNotFoundHandlers : IContentFinder
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
		public bool TryFindDocument(PublishedContentRequest docRequest)
        {
			HandlePageNotFound(docRequest);
            return docRequest.HasPublishedContent;
        }

		#region Copied over and adapted from presentation.requestHandler

		//FIXME: this is temporary and should be obsoleted

		void HandlePageNotFound(PublishedContentRequest docRequest)
        {			
			var url = NotFoundHandlerHelper.GetLegacyUrlForNotFoundHandlers();
            LogHelper.Debug<ContentFinderByNotFoundHandlers>("Running for legacy url='{0}'.", () => url);

            foreach (var handler in GetNotFoundHandlers())
            {
                IContentFinder finder = null;

                LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}'.", () => handler.GetType().FullName);

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
                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Replace handler '{0}' by new finder '{1}'.", () => handler.GetType().FullName, () => finder.GetType().FullName);
                    if (finder.TryFindDocument(docRequest))
                    {
                        // do NOT set docRequest.PublishedContent again here as 
                        // it would clear any template that the finder might have set
                        LogHelper.Debug<ContentFinderByNotFoundHandlers>("Finder '{0}' found node with id={1}.", () => finder.GetType().FullName, () => docRequest.PublishedContent.Id);
                        if (docRequest.Is404)
                            LogHelper.Debug<ContentFinderByNotFoundHandlers>("Finder '{0}' set status to 404.", () => finder.GetType().FullName);
                    }
                    break;
                }

                // else it's a legacy handler, run

				if (handler.Execute(url) && handler.redirectID > 0)
                {
                    docRequest.PublishedContent = docRequest.RoutingContext.PublishedContentStore.GetDocumentById(
						docRequest.RoutingContext.UmbracoContext,
						handler.redirectID);

                    if (!docRequest.HasPublishedContent)
                    {
                        LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' found node with id={1} which is not valid.", () => handler.GetType().FullName, () => handler.redirectID);
                        break;
                    }

                    LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' found valid node with id={1}.", () => handler.GetType().FullName, () => handler.redirectID);

                    if (docRequest.RoutingContext.UmbracoContext.HttpContext.Response.StatusCode == 404)
                    {
                        LogHelper.Debug<ContentFinderByNotFoundHandlers>("Handler '{0}' set status code to 404.", () => handler.GetType().FullName);
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

                    break;
                }
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
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
	/// Provides an implementation of <see cref="IDocumentLastChanceLookup"/> that handles backward compatilibty with legacy <c>INotFoundHandler</c>.
	/// </summary>
    internal class DefaultLastChanceLookup : IDocumentLastChanceLookup
    {

		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TrySetDocument(DocumentRequest docRequest)
        {
			docRequest.Document = HandlePageNotFound(docRequest);
            return docRequest.HasNode;
        }

		#region Copied over from presentation.requestHandler

		//FIXME: this is temporary and should be obsoleted

		IDocument HandlePageNotFound(DocumentRequest docRequest)
        {
			LogHelper.Debug<DefaultLastChanceLookup>("Running for url='{0}'.", () => docRequest.Uri.AbsolutePath);
			
			//XmlNode currentPage = null;
			IDocument currentPage = null;

            foreach (var handler in GetNotFoundHandlers())
            {
				if (handler.Execute(docRequest.Uri.AbsolutePath) && handler.redirectID > 0)
                {
                    //currentPage = umbracoContent.GetElementById(handler.redirectID.ToString());
					currentPage = docRequest.RoutingContext.PublishedContentStore.GetDocumentById(
						docRequest.RoutingContext.UmbracoContext,
						handler.redirectID);

                    // FIXME - could it be null?

					LogHelper.Debug<DefaultLastChanceLookup>("Handler '{0}' found node with id={1}.", () => handler.GetType().FullName, () => handler.redirectID);                    

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

			//SD: We are setting the Is404 to true here because these are 404 handlers. 
			// if people in the future add their own last chance lookup resolver, they might not want things to be 404s
			// and instead do their own thing so we should leave it up to the last chance resolver to set the 404, not the
			// module.
			docRequest.Is404 = true;

            return currentPage;
        }

        static IEnumerable<Type> _customHandlerTypes = null;
        static readonly object CustomHandlerTypesLock = new object();

        IEnumerable<Type> InitializeNotFoundHandlers()
        {
            // initialize handlers
            // create the definition cache

			LogHelper.Debug<DefaultLastChanceLookup>("Registering custom handlers.");                    

            var customHandlerTypes = new List<Type>();

            var customHandlers = new XmlDocument();
			customHandlers.Load(Umbraco.Core.IO.IOHelper.MapPath(Umbraco.Core.IO.SystemFiles.NotFoundhandlersConfig));

            foreach (XmlNode n in customHandlers.DocumentElement.SelectNodes("notFound"))
            {
                var assemblyName = n.Attributes.GetNamedItem("assembly").Value;

                // skip those that are in umbraco.dll because we have
                // replaced them with ILookups already -- so we just
                // want to load user-defined NotFound handlers...
                if (assemblyName == "umbraco")
                    continue;

                var typeName = n.Attributes.GetNamedItem("type").Value;
                string ns = assemblyName;
                var nsAttr = n.Attributes.GetNamedItem("namespace");
                if (nsAttr != null && !string.IsNullOrWhiteSpace(nsAttr.Value))
                    ns = nsAttr.Value;
                Type type = null;

				LogHelper.Debug<DefaultLastChanceLookup>("Registering '{0}.{1},{2}'.", () => ns, () => typeName, () => assemblyName);      
                
                try
                {
					//TODO: This isn't a good way to load the assembly, its already in the Domain so we should be getting the type
					// this loads the assembly into the wrong assembly load context!!

					var assembly = Assembly.LoadFrom(Umbraco.Core.IO.IOHelper.MapPath(Umbraco.Core.IO.SystemDirectories.Bin + "/" + assemblyName + ".dll"));
                    type = assembly.GetType(ns + "." + typeName);
                }
                catch (Exception e)
                {
					LogHelper.Error<DefaultLastChanceLookup>("Error registering handler, ignoring.", e);                       
                }

                if (type != null)
					customHandlerTypes.Add(type);
            }

        	return customHandlerTypes;
        }

        IEnumerable<INotFoundHandler> GetNotFoundHandlers()
        {
            // instanciate new handlers
            // using definition cache

            lock (CustomHandlerTypesLock)
            {
                if (_customHandlerTypes == null)
                    _customHandlerTypes = InitializeNotFoundHandlers();
            }

            var handlers = new List<INotFoundHandler>();

            foreach (var type in _customHandlerTypes)
            {
                try
                {
                    var handler = Activator.CreateInstance(type) as INotFoundHandler;
                    if (handler != null)
                        handlers.Add(handler);
                }
                catch (Exception e)
                {
					LogHelper.Error<DefaultLastChanceLookup>(string.Format("Error instanciating handler {0}, ignoring.", type.FullName), e);                         
                }
            }

            return handlers;
		}

		#endregion
	}
}
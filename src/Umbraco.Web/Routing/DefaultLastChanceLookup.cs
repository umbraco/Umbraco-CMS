using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Xml;
using Umbraco.Core.Logging;
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
			docRequest.Node = HandlePageNotFound(docRequest);
            return docRequest.HasNode;
        }

		#region Copied over from presentation.requestHandler

		//FIXME: this is temporary and should be obsoleted

		XmlNode HandlePageNotFound(DocumentRequest docRequest)
        {
			LogHelper.Debug<DefaultLastChanceLookup>("Running for url='{0}'.", () => docRequest.Uri.AbsolutePath);
			
            XmlNode currentPage = null;

            foreach (var handler in GetNotFoundHandlers())
            {
				if (handler.Execute(docRequest.Uri.AbsolutePath) && handler.redirectID > 0)
                {
                    //currentPage = umbracoContent.GetElementById(handler.redirectID.ToString());
					currentPage = docRequest.RoutingContext.ContentStore.GetNodeById(handler.redirectID);

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

            return currentPage;
        }

        static List<Type> _customHandlerTypes = null;
        static readonly object CustomHandlerTypesLock = new object();

        void InitializeNotFoundHandlers()
        {
            // initialize handlers
            // create the definition cache

			LogHelper.Debug<DefaultLastChanceLookup>("Registering custom handlers.");                    

            _customHandlerTypes = new List<Type>();

            var customHandlers = new XmlDocument();
            customHandlers.Load(IOHelper.MapPath(SystemFiles.NotFoundhandlersConfig));

            foreach (XmlNode n in customHandlers.DocumentElement.ChildNodes)
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

                    var assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + assemblyName + ".dll"));
                    type = assembly.GetType(ns + "." + typeName);
                }
                catch (Exception e)
                {
					LogHelper.Error<DefaultLastChanceLookup>("Error registering handler, ignoring.", e);                       
                }

                if (type != null)
                    _customHandlerTypes.Add(type);
            }
        }

        IEnumerable<INotFoundHandler> GetNotFoundHandlers()
        {
            // instanciate new handlers
            // using definition cache

            lock (CustomHandlerTypesLock)
            {
                if (_customHandlerTypes == null)
                    InitializeNotFoundHandlers();
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
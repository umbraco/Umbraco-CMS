using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Xml;
using umbraco.IO;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
    internal class LookupFor404 : ILookupNotFound
    {
        public LookupFor404(ContentStore contentStore)
        {
            _contentStore = contentStore;
        }

        static TraceSource _trace = new TraceSource("LookupByAlias");

        private readonly ContentStore _contentStore;

        public bool LookupDocument(DocumentRequest docRequest)
        {
			docRequest.Node = HandlePageNotFound(docRequest.Uri.AbsolutePath);
            return docRequest.HasNode;
        }

        // --------

        // copied from presentation/requestHandler
        // temporary!!
        XmlNode HandlePageNotFound(string url)
        {
            HttpContext.Current.Trace.Write("NotFoundHandler", string.Format("Running for url='{0}'.", url));
            XmlNode currentPage = null;

            foreach (var handler in GetNotFoundHandlers())
            {
                if (handler.Execute(url) && handler.redirectID > 0)
                {
                    //currentPage = umbracoContent.GetElementById(handler.redirectID.ToString());
                    currentPage = _contentStore.GetNodeById(handler.redirectID);

                    // FIXME - could it be null?

                    HttpContext.Current.Trace.Write("NotFoundHandler",
                                                    string.Format("Handler '{0}' found node with id={1}.", handler.GetType().FullName, handler.redirectID));

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

            HttpContext.Current.Trace.Write("NotFoundHandler", "Registering custom handlers.");

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

                HttpContext.Current.Trace.Write("NotFoundHandler",
                                                string.Format("Registering '{0}.{1},{2}'.", ns, typeName, assemblyName));

                try
                {
                    var assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + assemblyName + ".dll"));
                    type = assembly.GetType(ns + "." + typeName);
                }
                catch (Exception e)
                {
                    HttpContext.Current.Trace.Warn("NotFoundHandler", "Error registering handler, ignoring.", e);
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
                    HttpContext.Current.Trace.Warn("NotFoundHandler",
                                                   string.Format("Error instanciating handler {0}, ignoring.", type.FullName),
                                                   e);
                }
            }

            return handlers;
        }
    }
}
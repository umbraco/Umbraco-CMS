using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Reflection;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.interfaces;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;

namespace Umbraco.Web.Routing
{
    // provides internal access to legacy url -- should get rid of it eventually
    internal class NotFoundHandlerHelper
    {
        const string ContextKey = "Umbraco.Web.Routing.NotFoundHandlerHelper.Url";

        static NotFoundHandlerHelper()
        {
            InitializeNotFoundHandlers();
        }

        public static string GetLegacyUrlForNotFoundHandlers()
        {
            // that's not backward-compatible because when requesting "/foo.aspx"
            // 4.9  : url = "foo.aspx"
            // 4.10 : url = "/foo"
            //return pcr.Uri.AbsolutePath;

            // so we have to run the legacy code for url preparation :-(

            var httpContext = HttpContext.Current;

            if (httpContext == null)
                return "";

            var url = httpContext.Items[ContextKey] as string;
            if (url != null)
                return url;

            // code from requestModule.UmbracoRewrite
            var tmp = httpContext.Request.Path.ToLower();

            // note: requestModule.UmbracoRewrite also did some stripping of &umbPage
            // from the querystring... that was in v3.x to fix some issues with pre-forms
            // auth. Paul Sterling confirmed in jan. 2013 that we can get rid of it.

            // code from requestHandler.cleanUrl
            var root = Core.IO.SystemDirectories.Root.ToLower();
            if (!string.IsNullOrEmpty(root) && tmp.StartsWith(root))
                tmp = tmp.Substring(root.Length);
            tmp = tmp.TrimEnd('/');
            if (tmp == "/default.aspx")
                tmp = string.Empty;
            else if (tmp == root)
                tmp = string.Empty;

            // code from UmbracoDefault.Page_PreInit
            if (tmp != "" && httpContext.Request["umbPageID"] == null)
            {
                var tryIntParse = tmp.Replace("/", "").Replace(".aspx", string.Empty);
                int result;
                if (int.TryParse(tryIntParse, out result))
                    tmp = tmp.Replace(".aspx", string.Empty);
            }
            else if (!string.IsNullOrEmpty(httpContext.Request["umbPageID"]))
            {
                int result;
                if (int.TryParse(httpContext.Request["umbPageID"], out result))
                {
                    tmp = httpContext.Request["umbPageID"];
                }
            }

            // code from requestHandler.ctor
            if (tmp != "")
                tmp = tmp.Substring(1);

            httpContext.Items[ContextKey] = tmp;
            return tmp;
        }

        private static IEnumerable<Type> _customHandlerTypes;
        private static Type _customLastChanceHandlerType;

        static void InitializeNotFoundHandlers()
        {
            // initialize handlers
            // create the definition cache

            LogHelper.Debug<NotFoundHandlerHelper>("Registering custom handlers.");

            var customHandlerTypes = new List<Type>();
            Type customHandlerType = null;

            var customHandlers = new XmlDocument();
            customHandlers.Load(Core.IO.IOHelper.MapPath(Core.IO.SystemFiles.NotFoundhandlersConfig));

            foreach (XmlNode n in customHandlers.DocumentElement.SelectNodes("notFound"))
            {
                if (customHandlerType != null)
                {
                    LogHelper.Debug<NotFoundHandlerHelper>("Registering '{0}'.", () => customHandlerType.FullName);
                    customHandlerTypes.Add(customHandlerType);
                }

                var assemblyName = n.Attributes.GetNamedItem("assembly").Value;
                var typeName = n.Attributes.GetNamedItem("type").Value;

                var ns = assemblyName;
                var nsAttr = n.Attributes.GetNamedItem("namespace");
                if (nsAttr != null && string.IsNullOrWhiteSpace(nsAttr.Value) == false)
                    ns = nsAttr.Value;

                LogHelper.Debug<NotFoundHandlerHelper>("Configured: '{0}.{1},{2}'.", () => ns, () => typeName, () => assemblyName);

                customHandlerType = null;
                try
                {
                    var assembly = Assembly.Load(new AssemblyName(assemblyName));
                    customHandlerType = assembly.GetType(ns + "." + typeName);
                }
                catch (Exception e)
                {
                    LogHelper.Error<NotFoundHandlerHelper>("Error: could not load handler, ignoring.", e);
                }
            }

            // what shall we do with the last one, assuming it's not null?
            // if the last chance finder wants a handler, then use the last one as the last chance handler
            // else assume that the last one is a normal handler since noone else wants it, and add it to the list
            if (customHandlerType != null)
            {
                var lastChanceFinder = ContentLastChanceFinderResolver.Current.Finder; // can be null
                var finderWantsHandler = lastChanceFinder != null &&
                    lastChanceFinder.GetType() == typeof(ContentLastChanceFinderByNotFoundHandlers);

                if (finderWantsHandler)
                {
                    LogHelper.Debug<NotFoundHandlerHelper>("Registering '{0}' as \"last chance\" handler.", () => customHandlerType.FullName);
                    _customLastChanceHandlerType = customHandlerType;
                }
                else
                {
                    LogHelper.Debug<NotFoundHandlerHelper>("Registering '{0}'.", () => customHandlerType.FullName);
                    customHandlerTypes.Add(customHandlerType);
                    _customLastChanceHandlerType = null;
                }
            }

            _customHandlerTypes = customHandlerTypes.ToArray();
        }

        public static IEnumerable<INotFoundHandler> GetNotFoundHandlers()
        {
            // instanciate new handlers
            // using definition cache

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
                    LogHelper.Error<ContentFinderByNotFoundHandlers>(string.Format("Error instanciating handler {0}, ignoring.", type.FullName), e);
                }
            }

            return handlers;
        }

        public static bool IsNotFoundHandlerEnabled<T>()
        {
            return _customHandlerTypes.Contains(typeof(T));
        }

        public static INotFoundHandler GetNotFoundLastChanceHandler()
        {
            if (_customLastChanceHandlerType == null) return null;

            try
            {
                var handler = Activator.CreateInstance(_customLastChanceHandlerType) as INotFoundHandler;
                if (handler != null)
                    return handler;
            }
            catch (Exception e)
            {
                LogHelper.Error<ContentFinderByNotFoundHandlers>(string.Format("Error instanciating handler {0}, ignoring.", _customLastChanceHandlerType.FullName), e);
            }

            return null;
        }

        public static IContentFinder SubsituteFinder(INotFoundHandler handler)
        {
            IContentFinder finder = null;

            if (handler is global::umbraco.SearchForAlias)
                finder = new ContentFinderByUrlAlias();
            else if (handler is global::umbraco.SearchForProfile)
                finder = new ContentFinderByProfile();
            else if (handler is global::umbraco.SearchForTemplate)
                finder = new ContentFinderByNiceUrlAndTemplate();
            else if (handler is global::umbraco.handle404)
                finder = new ContentFinderByLegacy404();

            return finder;
        }

        /// <summary>
        /// Returns the Umbraco page id to use as the Not Found page based on the configured 404 pages and the current request
        /// </summary>
        /// <param name="error404Collection"></param>
        /// <param name="requestServerName">
        /// The server name attached to the request, normally would be the source of HttpContext.Current.Request.ServerVariables["SERVER_NAME"]
        /// </param>
        /// <param name="entityService"></param>
        /// <param name="publishedContentQuery"></param>
        /// <param name="domainService"></param>
        /// <returns></returns>
        internal static int? GetCurrentNotFoundPageId(
            IContentErrorPage[] error404Collection, 
            string requestServerName, 
            IEntityService entityService,
            ITypedPublishedContentQuery publishedContentQuery,
            IDomainService domainService)
        {
            if (error404Collection.Count() > 1)
            {
                // try to get the 404 based on current culture (via domain)
                IContentErrorPage cultureErr;

                //TODO: Remove the dependency on this legacy Domain service, 
                // in 7.3 the real domain service should be passed in as a parameter.
                if (domainService.Exists(requestServerName))
                {
                    var d = domainService.GetByName(requestServerName);

                    // test if a 404 page exists with current culture
                    cultureErr = error404Collection
                        .FirstOrDefault(x => x.Culture == d.Language.IsoCode);

                    if (cultureErr != null)
                    {
                        return GetContentIdFromErrorPageConfig(cultureErr, entityService, publishedContentQuery);
                    }
                }

                // test if a 404 page exists with current culture thread
                cultureErr = error404Collection
                    .FirstOrDefault(x => x.Culture == System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                if (cultureErr != null)
                {
                    return GetContentIdFromErrorPageConfig(cultureErr, entityService, publishedContentQuery);
                }

                // there should be a default one!
                cultureErr = error404Collection
                    .FirstOrDefault(x => x.Culture == "default");

                if (cultureErr != null)
                {
                    return GetContentIdFromErrorPageConfig(cultureErr, entityService, publishedContentQuery);
                }
            }
            else
            {
                return GetContentIdFromErrorPageConfig(error404Collection.First(), entityService, publishedContentQuery);
            }

            return null;
        }

        /// <summary>
        /// Returns the content id based on the configured IContentErrorPage section
        /// </summary>
        /// <param name="errorPage"></param>
        /// <param name="entityService"></param>
        /// <param name="publishedContentQuery"></param>
        /// <returns></returns>
        internal static int? GetContentIdFromErrorPageConfig(IContentErrorPage errorPage, IEntityService entityService, ITypedPublishedContentQuery publishedContentQuery)
        {
            if (errorPage.HasContentId) return errorPage.ContentId;

            if (errorPage.HasContentKey)
            {
                //need to get the Id for the GUID
                //TODO: When we start storing GUIDs into the IPublishedContent, then we won't have to look this up 
                // but until then we need to look it up in the db. For now we've implemented a cached service for 
                // converting Int -> Guid and vice versa.
                var found = entityService.GetIdForKey(errorPage.ContentKey, UmbracoObjectTypes.Document);
                if (found)
                {
                    return found.Result;
                }
                return null;
            }

            if (errorPage.ContentXPath.IsNullOrWhiteSpace() == false)
            {
                try
                {
                    //we have an xpath statement to execute
                    var xpathResult = UmbracoXPathPathSyntaxParser.ParseXPathQuery(
                        xpathExpression: errorPage.ContentXPath,
                        nodeContextId: null,
                        getPath: nodeid =>
                        {
                            var ent = entityService.Get(nodeid);
                            return ent.Path.Split(',').Reverse();
                        },
                        publishedContentExists: i => publishedContentQuery.TypedContent(i) != null);

                    //now we'll try to execute the expression
                    var nodeResult = publishedContentQuery.TypedContentSingleAtXPath(xpathResult);
                    if (nodeResult != null) 
                        return nodeResult.Id;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<NotFoundHandlerHelper>("Could not parse xpath expression: " + errorPage.ContentXPath, ex);
                    return null;
                }
            }
            return null;
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using Umbraco.Core;
using umbraco.IO;
using umbraco.NodeFactory;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;

namespace umbraco
{
    /// <summary>
    /// Summary description for requestHandler.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed in future versions")]
    public class requestHandler
    {
        #region public properties

        public XmlNode currentPage;

        public String PageXPathQuery
        {
            get { return _pageXPathQuery; }
        }

        #endregion

        private static Hashtable _processedRequests = new Hashtable();
        private static object locker = new object();


        private static string pageXPathQueryStart = "/root";
        private static string _urlName = "@urlName";
        private static bool _urlNameInitialized = true;
        private static XmlDocument _customHandlers;

        private string _pageXPathQuery = string.Empty;
        private bool _doNotCache = false;

        public static void ClearProcessedRequests()
        {
            lock (locker)
            {
                _processedRequests.Clear();
            }
        }

        public static string cleanUrl()
        {
            if (HttpContext.Current.Items["UmbPage"] == null)
                return string.Empty;

            string tmp = HttpContext.Current.Items["UmbPage"].ToString();
            string root = SystemDirectories.Root.ToLower();

            //if we are running in a virtual dir
            if (!string.IsNullOrEmpty(root) && tmp.StartsWith(root))
            {
                tmp = tmp.Substring(root.Length);
            }

            // Remove the Trailing slash.  
            tmp = tmp.TrimEnd('/');

            if (tmp == "/default.aspx")
                tmp = string.Empty;
            //            else if (tmp == "/")
            //                tmp = string.Empty;
            else if (tmp == root)
                tmp = string.Empty;

            return tmp;
        }

        // Init urlName to correspond to web.config entries (umbracoUrlForbittenCharacters and umbracoUrlSpaceCharacter).
        // Needed to compensate for known asp.net framework error KB826437:
        // http://support.microsoft.com/default.aspx?scid=kb;EN-US;826437
        // note: obsoleted, everything was commented out anyway since long, so it just
        //   initializes _urlName, which we can do in the variable definition.
        [Obsolete("This method did nothing anyway...")]
        private static void InitializeUrlName()
        {
            /*			string toReplace = string.Empty;
			string replaceWith = string.Empty;
			XmlNode replaceChars = UmbracoSettings.UrlReplaceCharacters;
			foreach (XmlNode n in replaceChars.SelectNodes("char")) 
			{
				if (xmlHelper.GetNodeValue(n).Trim() != string.Empty) 
				{
					toReplace += n.Attributes.GetNamedItem("org").Value;
					replaceWith += n.FirstChild.Value.Trim();
				}
			}
			_urlName = "translate(@urlName, '" + toReplace + "','" + replaceWith + "')";

*/
            _urlName = "@urlName";
            _urlNameInitialized = true;
        }

        public static string CreateXPathQuery(string url, bool checkDomain)
        {

            string _tempQuery = "";
            if (GlobalSettings.HideTopLevelNodeFromPath && checkDomain)
            {
                _tempQuery = "/root" + getChildContainerName() + "/*";
            }
            else if (checkDomain)
                _tempQuery = "/root" + getChildContainerName();


            string[] requestRawUrl = url.Split("/".ToCharArray());

            // Check for Domain prefix
            string domainUrl = "";
            if (checkDomain && Domain.Exists(HttpContext.Current.Request.ServerVariables["SERVER_NAME"]))
            {
                // we need to get the node based on domain
                INode n = new Node(Domain.GetRootFromDomain(HttpContext.Current.Request.ServerVariables["SERVER_NAME"]));
                domainUrl = n.UrlName; // we don't use niceUrlFetch as we need more control
                if (n.Parent != null)
                {
                    while (n.Parent != null)
                    {
                        n = n.Parent;
                        domainUrl = n.UrlName + "/" + domainUrl;
                    }
                }
                domainUrl = "/" + domainUrl;

                // If at domain root
                if (url == "")
                {
                    _tempQuery = "";
                    requestRawUrl = domainUrl.Split("/".ToCharArray());
                    HttpContext.Current.Trace.Write("requestHandler",
                                                    "Redirecting to domain: " +
                                                    HttpContext.Current.Request.ServerVariables["SERVER_NAME"] +
                                                    ", nodeId: " +
                                                    Domain.GetRootFromDomain(
                                                        HttpContext.Current.Request.ServerVariables["SERVER_NAME"]).
                                                        ToString());
                }
                else
                {
                    // if it matches a domain url, skip all other xpaths and use this!
                    string langXpath = CreateXPathQuery(domainUrl + "/" + url, false);
                    if (content.Instance.XmlContent.DocumentElement.SelectSingleNode(langXpath) != null)
                        return langXpath;
                    else if (UmbracoSettings.UseDomainPrefixes)
                        return "/domainprefixes-are-used-so-i-do-not-work";
                }
            }
            else if (url == "" && !GlobalSettings.HideTopLevelNodeFromPath)
                _tempQuery += "/*";

            bool rootAdded = false;
            if (GlobalSettings.HideTopLevelNodeFromPath && requestRawUrl.Length == 1)
            {
                HttpContext.Current.Trace.Write("umbracoRequestHandler", "xpath: '" + _tempQuery + "'");
                if (_tempQuery == "")
                    _tempQuery = "/root" + getChildContainerName() + "/*";
                _tempQuery = "/root" + getChildContainerName() + "/* [" + _urlName +
                             " = \"" + requestRawUrl[0].Replace(".aspx", "").ToLower() + "\"] | " + _tempQuery;
                HttpContext.Current.Trace.Write("umbracoRequestHandler", "xpath: '" + _tempQuery + "'");
                rootAdded = true;
            }


            for (int i = 0; i <= requestRawUrl.GetUpperBound(0); i++)
            {
                if (requestRawUrl[i] != "")
                    _tempQuery += getChildContainerName() + "/* [" + _urlName + " = \"" + requestRawUrl[i].Replace(".aspx", "").ToLower() +
                                  "\"]";
            }

            if (GlobalSettings.HideTopLevelNodeFromPath && requestRawUrl.Length == 2)
            {
                _tempQuery += " | " + pageXPathQueryStart + getChildContainerName() + "/* [" + _urlName + " = \"" +
                              requestRawUrl[1].Replace(".aspx", "").ToLower() + "\"]";
            }
            HttpContext.Current.Trace.Write("umbracoRequestHandler", "xpath: '" + _tempQuery + "'");

            Debug.Write(_tempQuery + "(" + pageXPathQueryStart + ")");

            if (checkDomain)
                return _tempQuery;
            else if (!rootAdded)
                return pageXPathQueryStart + _tempQuery;
            else
                return _tempQuery;
        }

        private static string getChildContainerName()
        {
            if (!String.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME))
                return "/" + UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME;
            else
                return "";
        }

        public requestHandler(XmlDocument umbracoContent, String url)
        {
            HttpContext.Current.Trace.Write("request handler", "current url '" + url + "'");
            bool getByID = false;
            string currentDomain = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];

            // obsoleted
            //if (!_urlNameInitialized)
            //    InitializeUrlName();

            // The url exists in cache, and the domain doesn't exists (which makes it ok to do a cache look up on the url alone)
            // TODO: NH: Remove the flag for friendlyxmlschema when real schema is implemented
            // as the friendlyxmlschema doesn't have any identifier keys we can't do a lookup
            // based on ID yet.
            if (_processedRequests.ContainsKey(url) && !Domain.Exists(currentDomain))
            {
                getByID = true;
                _pageXPathQuery = _processedRequests[url].ToString();
            }
            // The url including the domain exists in cache
            else if (_processedRequests.ContainsKey(currentDomain + url))
            {
                getByID = true;
                _pageXPathQuery = _processedRequests[currentDomain + url].ToString();
            }
            // The url isn't cached
            else
            {
                if (url == "")
                {
                    url = "";
                    _pageXPathQuery = CreateXPathQuery(url, true);

                    // Never cache roots
                    _doNotCache = true;
                }
                else
                {
                    // If url is an integer, then asume it's the ID of the page
                    if (url[0] == '/')
                        url = url.Substring(1, url.Length - 1);
                    int result;
                    if (int.TryParse(url, out result))
                    {
                        _pageXPathQuery = url;
                        getByID = true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(url))
                        {
                            _pageXPathQuery = CreateXPathQuery(url, true);
                        }
                        else
                        {
                            _pageXPathQuery = Document.GetRootDocuments()[0].Id.ToString();
                            getByID = true;
                        }
                    }
                }
            }

            HttpContext.Current.Trace.Write("umbracoRequestHandler",
                                            string.Format("Just before xPath query ({0}, '{1}')", getByID,
                                                          _pageXPathQuery));

            if (getByID)
                currentPage = umbracoContent.GetElementById(_pageXPathQuery.Trim());
            else
            {
                HttpContext.Current.Trace.Write("umbracoRequestHandler",
                                                "pageXPathQueryStart: '" + pageXPathQueryStart + "'");
                currentPage = umbracoContent.SelectSingleNode(pageXPathQueryStart + _pageXPathQuery);
                if (currentPage == null)
                {
                    // If no node found, then try with a relative page query
                    currentPage = umbracoContent.SelectSingleNode("/" + _pageXPathQuery);
                }

                // Add to url cache
                if (currentPage != null && !_doNotCache)
                {
                    string prefixUrl = "";
                    if (Domain.Exists(currentDomain))
                        prefixUrl = currentDomain;
                    if (url.Substring(0, 1) != "/")
                        url = "/" + url;

                    lock (locker)
                    {
                        if (!_processedRequests.ContainsKey(prefixUrl + url))
                            _processedRequests.Add(prefixUrl + url, currentPage.Attributes.GetNamedItem("id").Value);
                    }


                    HttpContext.Current.Trace.Write("umbracoRequestHandler",
                                                    "Adding to cache... ('" + prefixUrl + url + "')");
                }
            }

            if (currentPage == null)
            {
                // No node found, try custom url handlers defined in /config/404handlers.config
                if (_customHandlers == null)
                {
                    _customHandlers = new XmlDocument();
                    _customHandlers.Load(
                    IOHelper.MapPath(SystemFiles.NotFoundhandlersConfig));
                }

                foreach (XmlNode notFoundHandler in _customHandlers.DocumentElement.SelectNodes("notFound"))
                {

                    // Load handler
                    string _chAssembly = notFoundHandler.Attributes.GetNamedItem("assembly").Value;
                    string _chType = notFoundHandler.Attributes.GetNamedItem("type").Value;
                    // check for namespace
                    string _chNameSpace = _chAssembly;
                    if (notFoundHandler.Attributes.GetNamedItem("namespace") != null)
                        _chNameSpace = notFoundHandler.Attributes.GetNamedItem("namespace").Value;

                    try
                    {
                        // Reflect to execute and check whether the type is umbraco.main.IFormhandler
                        HttpContext.Current.Trace.Write("notFoundHandler",
                                                string.Format("Trying NotFoundHandler '{0}.{1}'...", _chAssembly, _chType));
                        Assembly assembly =
                            Assembly.LoadFrom(
                                IOHelper.MapPath(SystemDirectories.Bin + "/" + _chAssembly + ".dll"));

                        Type type = assembly.GetType(_chNameSpace + "." + _chType);
                        INotFoundHandler typeInstance = Activator.CreateInstance(type) as INotFoundHandler;
                        if (typeInstance != null)
                        {
                            typeInstance.Execute(url);
                            if (typeInstance.redirectID > 0)
                            {
                                int redirectID = typeInstance.redirectID;
                                currentPage = umbracoContent.GetElementById(redirectID.ToString());
                                HttpContext.Current.Trace.Write("notFoundHandler",
                                                string.Format("NotFoundHandler '{0}.{1} found node matching {2} with id: {3}",
                                                              _chAssembly, _chType, url, redirectID));

                                // check for caching
                                if (typeInstance.CacheUrl)
                                {
                                    if (url.Substring(0, 1) != "/")
                                        url = "/" + url;

                                    string prefixUrl = string.Empty;

                                    if (Domain.Exists(currentDomain))
                                        prefixUrl = currentDomain;
                                    if (url.Substring(0, 1) != "/")
                                        url = "/" + url;

                                    lock (locker)
                                    {
                                        if (!_processedRequests.ContainsKey(prefixUrl + url))
                                            _processedRequests.Add(prefixUrl + url, currentPage.Attributes.GetNamedItem("id").Value);
                                    }

                                    HttpContext.Current.Trace.Write("notFoundHandler",
                                                                    string.Format("Added to cache '{0}', '{1}'...", url,
                                                                                  redirectID));
                                }
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        HttpContext.Current.Trace.Warn("notFoundHandler",
                                                       "Error implementing notfoundHandler '" + _chAssembly + "." +
                                                       _chType + "'", e);
                    }
                }
            }

            HttpContext.Current.Trace.Write("umbracoRequestHandler", "After xPath query");

            // Check for internal redirects
            if (currentPage != null)
            {
                XmlNode internalRedirect = currentPage.SelectSingleNode(UmbracoSettings.UseLegacyXmlSchema ? string.Format("data [@alias = '{0}']", Constants.Conventions.Content.InternalRedirectId) : Constants.Conventions.Content.InternalRedirectId);
                if (internalRedirect != null && internalRedirect.FirstChild != null && !String.IsNullOrEmpty(internalRedirect.FirstChild.Value))
                {
                    HttpContext.Current.Trace.Write("internalRedirection", "Found internal redirect id via umbracoInternalRedirectId property alias");
                    int internalRedirectId = 0;
                    if (int.TryParse(internalRedirect.FirstChild.Value, out internalRedirectId) && internalRedirectId > 0)
                    {
                        currentPage =
                umbracoContent.GetElementById(
                    internalRedirectId.ToString());
                        HttpContext.Current.Trace.Write("internalRedirection", "Redirecting to " + internalRedirect.FirstChild.Value);
                    }
                    else
                        HttpContext.Current.Trace.Warn("internalRedirection", "The redirect id is not an integer: " + internalRedirect.FirstChild.Value);

                }
            }

            // Check access
            HttpContext.Current.Trace.Write("umbracoRequestHandler", "Access checking started");
            if (currentPage != null)
            {
                int id = int.Parse(currentPage.Attributes.GetNamedItem("id").Value);
                string path = currentPage.Attributes.GetNamedItem("path").Value;

                if (Access.IsProtected(id, path))
                {
                    HttpContext.Current.Trace.Write("umbracoRequestHandler", "Page protected");

                    var user = System.Web.Security.Membership.GetUser();

                    if (user == null || !library.IsLoggedOn())
                    {
                        HttpContext.Current.Trace.Write("umbracoRequestHandler", "Not logged in - redirecting to login page...");
                        currentPage = umbracoContent.GetElementById(Access.GetLoginPage(path).ToString());
                    }
                    else
                    {

                        if (user != null && !Access.HasAccces(id, user.ProviderUserKey))
                        {

                            HttpContext.Current.Trace.Write("umbracoRequestHandler", "Member has not access - redirecting to error page...");
                            currentPage = content.Instance.XmlContent.GetElementById(Access.GetErrorPage(path).ToString());
                        }
                    }
                }
                else
                    HttpContext.Current.Trace.Write("umbracoRequestHandler", "Page not protected");
            }
            HttpContext.Current.Trace.Write("umbracoRequestHandler", "Access checking ended");


        }
    }

    [Obsolete("This has been replaced with ScriptingMacroResult instead")]
    public class DLRMacroResult
    {
        public DLRMacroResult()
        {
        }

        public DLRMacroResult(Control control, Exception resultException)
        {
            Control = control;
            ResultException = resultException;
        }

        public Control Control { get; set; }
        public Exception ResultException { get; set; }
    }

    [Obsolete("This class is no longer used, use Umbraco.Web.Cache.MacroCacheRefresher instead")]
    public class macroCacheRefresh : Umbraco.Web.Cache.MacroCacheRefresher
    {

    }

    [Obsolete("This class is no longer used, use Umbraco.Web.Cache.LibraryCacheRefresher instead")]
    public class LibraryCacheRefresher : Umbraco.Web.Cache.CacheRefresherEventHandler
    {

    }

    [Obsolete("This class is no longer used, use Umbraco.Web.Cache.TemplateCacheRefresher instead")]
    public class templateCacheRefresh : Umbraco.Web.Cache.TemplateCacheRefresher
    {

    }
}

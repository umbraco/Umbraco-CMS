using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using Umbraco.Core.IO;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace umbraco {


    /// <summary>
    /// THIS CLASS IS PURELY HERE TO SUPPORT THE QUERYBYXPATH METHOD WHICH IS USED BY OTHER LEGACY BITS
    /// </summary>    
    internal class LegacyRequestHandler
    {
        private const string PageXPathQueryStart = "/root";
        private const string UrlName = "@urlName";
        
        public static string CreateXPathQuery(string url, bool checkDomain)
        {

            string _tempQuery = "";
            if (GlobalSettings.HideTopLevelNodeFromPath && checkDomain)
            {
                _tempQuery = "/root" + GetChildContainerName() + "/*";
            }
            else if (checkDomain)
                _tempQuery = "/root" + GetChildContainerName();


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
                    else if (UmbracoConfig.For.UmbracoSettings().RequestHandler.UseDomainPrefixes)
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
                    _tempQuery = "/root" + GetChildContainerName() + "/*";
                _tempQuery = "/root" + GetChildContainerName() + "/* [" + UrlName +
                             " = \"" + requestRawUrl[0].Replace(".aspx", "").ToLower() + "\"] | " + _tempQuery;
                HttpContext.Current.Trace.Write("umbracoRequestHandler", "xpath: '" + _tempQuery + "'");
                rootAdded = true;
            }


            for (int i = 0; i <= requestRawUrl.GetUpperBound(0); i++)
            {
                if (requestRawUrl[i] != "")
                    _tempQuery += GetChildContainerName() + "/* [" + UrlName + " = \"" + requestRawUrl[i].Replace(".aspx", "").ToLower() +
                                  "\"]";
            }

            if (GlobalSettings.HideTopLevelNodeFromPath && requestRawUrl.Length == 2)
            {
                _tempQuery += " | " + PageXPathQueryStart + GetChildContainerName() + "/* [" + UrlName + " = \"" +
                              requestRawUrl[1].Replace(".aspx", "").ToLower() + "\"]";
            }
            HttpContext.Current.Trace.Write("umbracoRequestHandler", "xpath: '" + _tempQuery + "'");

            Debug.Write(_tempQuery + "(" + PageXPathQueryStart + ")");

            if (checkDomain)
                return _tempQuery;
            else if (!rootAdded)
                return PageXPathQueryStart + _tempQuery;
            else
                return _tempQuery;
        }

        private static string GetChildContainerName()
        {
            if (string.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME) == false)
                return "/" + UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME;
            return "";
        }
    }

    public class SearchForAlias : INotFoundHandler {
        private int _redirectID = -1;
        private bool _cacheUrl = true;

        #region INotFoundHandler Members

        public bool Execute(string url) {
            bool _succes = false;
            string tempUrl = url.Trim('/').Replace(".aspx", string.Empty).ToLower();
            if (tempUrl.Length > 0) {
                HttpContext.Current.Trace.Write("urlAlias", "'" + tempUrl + "'");

                // Check for domain
                string currentDomain = System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                string prefixXPath = "";
                if (Domain.Exists(currentDomain)) {
                    string xpathDomain = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "//node [@id = '{0}']" : "//* [@isDoc and @id = '{0}']";
                    prefixXPath = string.Format(xpathDomain, Domain.GetRootFromDomain(currentDomain));
                    _cacheUrl = false;
                }


                // the reason we have almost two identical queries in the xpath is to support scenarios where the user have 
                // entered "/my-url" instead of "my-url" (ie. added a beginning slash)
                string xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "//node [contains(concat(',',translate(data [@alias = 'umbracoUrlAlias'], ' ', ''),','),',{0},') or contains(concat(',',translate(data [@alias = 'umbracoUrlAlias'], ' ', ''),','),',{1},')]" :
                    "//* [@isDoc and (contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',{0},') or contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',{1},'))]";
                string query = String.Format(prefixXPath + xpath, tempUrl, "/" + tempUrl);
                XmlNode redir =
                    content.Instance.XmlContent.DocumentElement.SelectSingleNode(query);
                if (redir != null) {
                    _succes = true;
                    _redirectID = int.Parse(redir.Attributes.GetNamedItem("id").Value);
                }
            }
            return _succes;
        }

        public bool CacheUrl {
            get { return _cacheUrl; }
        }

        public int redirectID {
            get {
                // TODO:  Add SearchForAlias.redirectID getter implementation
                return _redirectID;
            }
        }

        #endregion
    }

    public class SearchForProfile : INotFoundHandler {
        private static int _profileId = -1;

        private int _redirectID = -1;

        #region INotFoundHandler Members

        public bool CacheUrl {
            get {
                // Do not cache profile urls, we need to store the login name
                return false;
            }
        }

        public bool Execute(string url) {
            bool _succes = false;
            url = url.Replace(".aspx", string.Empty);
            if (url.Length > 0) {
                if (url.Substring(0, 1) == "/")
                    url = url.Substring(1, url.Length - 1);

                if (url.IndexOf("/") > 0) {
                    // Check if we're at the profile page
                    if (url.Substring(0, url.IndexOf("/")) == GlobalSettings.ProfileUrl) {
                        if (_profileId < 0) {
                            // /root added to query to solve umbRuntime bug
                            string _tempQuery =
                                LegacyRequestHandler.CreateXPathQuery(url.Substring(0, url.IndexOf("/")), false);
                            _profileId =
                                int.Parse(
                                    content.Instance.XmlContent.SelectSingleNode(_tempQuery).Attributes.GetNamedItem(
                                        "id").Value);
                        }

                        HttpContext.Current.Items["umbMemberLogin"] =
                            url.Substring(url.IndexOf("/") + 1, url.Length - url.IndexOf("/") - 1);
                        _succes = true;
                        _redirectID = _profileId;
                    }
                }
            }
            return _succes;
        }

        public int redirectID {
            get {
                // TODO:  Add SearchForProfile.redirectID getter implementation
                return _redirectID;
            }
        }

        #endregion
    }

    public class SearchForTemplate : INotFoundHandler {
        private int _redirectID = -1;

        #region INotFoundHandler Members

        public bool CacheUrl {
            get {
                // Do not cache profile urls, we need to store the login name
                return false;
            }
        }

        public bool Execute(string url) {
            bool _succes = false;
            url = url.Replace(".aspx", string.Empty);
            string currentDomain = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            if (url.Length > 0) {
                if (url.Substring(0, 1) == "/")
                    url = url.Substring(1, url.Length - 1);

                XmlNode urlNode = null;
                string templateAlias = "";

                // We're at domain root
                if (url.IndexOf("/") == -1)
                {
                    if (Domain.Exists(currentDomain))
                        urlNode = content.Instance.XmlContent.GetElementById(Domain.GetRootFromDomain(currentDomain).ToString());
                    else
                        urlNode = content.Instance.XmlContent.GetElementById(Document.GetRootDocuments()[0].Id.ToString());
                    templateAlias = url.ToLower();
                }
                else
                {
                    string theRealUrl = url.Substring(0, url.LastIndexOf("/"));
                    string realUrlXPath = LegacyRequestHandler.CreateXPathQuery(theRealUrl, true);
                    urlNode = content.Instance.XmlContent.SelectSingleNode(realUrlXPath);
                    templateAlias =
                        url.Substring(url.LastIndexOf("/") + 1, url.Length - url.LastIndexOf(("/")) - 1).ToLower();
                }

                if (urlNode != null && Template.GetTemplateIdFromAlias(templateAlias) != 0)
                {
                    _redirectID = int.Parse(urlNode.Attributes.GetNamedItem("id").Value);

                    if (UmbracoConfig.For.UmbracoSettings().WebRouting.DisableAlternativeTemplates == false)
                    {
                        HttpContext.Current.Items[Constants.Conventions.Url.AltTemplate] = templateAlias;
                        HttpContext.Current.Trace.Write("umbraco.altTemplateHandler",
                            string.Format("Template changed to: '{0}'", HttpContext.Current.Items[Constants.Conventions.Url.AltTemplate]));
                    }

                    _succes = true;
                }
            }
            return _succes;
        }

        public int redirectID {
            get {
                // TODO:  Add SearchForProfile.redirectID getter implementation
                return _redirectID;
            }
        }

        #endregion
    }

    public class handle404 : INotFoundHandler {
        #region INotFoundHandler Members

        private int _redirectID = 0;

        public bool CacheUrl {
            get {
                return false;
            }
        }

        public bool Execute(string url) {
			try
			{
                LogHelper.Info<handle404>(string.Format("NotFound url {0} (from '{1}')", url, HttpContext.Current.Request.UrlReferrer));

                // Test if the error404 not child elements
			    var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
			        UmbracoConfig.For.UmbracoSettings().Content.Error404Collection.ToArray(),
			        HttpContext.Current.Request.ServerVariables["SERVER_NAME"],
			        ApplicationContext.Current.Services.EntityService,
			        new PublishedContentQuery(UmbracoContext.Current.ContentCache, UmbracoContext.Current.MediaCache),
                    ApplicationContext.Current.Services.DomainService);

			    if (error404.HasValue)
			    {
			        _redirectID = error404.Value;
			        HttpContext.Current.Response.StatusCode = 404;
			        return true;
			    }

			    return false;
			}
			catch (Exception err)
			{
				LogHelper.Error<handle404>("An error occurred", err);
				return false;
			}
        }

        public int redirectID {
            get {
                return _redirectID;
            }
        }

        #endregion
    }
}
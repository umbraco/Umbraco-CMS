using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Xml;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.IO;
using umbraco.NodeFactory;

namespace umbraco {
    

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
                    string xpathDomain = UmbracoSettings.UseLegacyXmlSchema ? "//node [@id = '{0}']" : "//* [@isDoc and @id = '{0}']";
                    prefixXPath = string.Format(xpathDomain, Domain.GetRootFromDomain(currentDomain));
                    _cacheUrl = false;
                }


                // the reason we have almost two identical queries in the xpath is to support scenarios where the user have 
                // entered "/my-url" instead of "my-url" (ie. added a beginning slash)
                string xpath = UmbracoSettings.UseLegacyXmlSchema ? "//node [contains(concat(',',translate(data [@alias = 'umbracoUrlAlias'], ' ', ''),','),',{0},') or contains(concat(',',translate(data [@alias = 'umbracoUrlAlias'], ' ', ''),','),',{1},')]" :
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
                                requestHandler.CreateXPathQuery(url.Substring(0, url.IndexOf("/")), false);
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
                    string realUrlXPath = requestHandler.CreateXPathQuery(theRealUrl, true);
                    urlNode = content.Instance.XmlContent.SelectSingleNode(realUrlXPath);
                    templateAlias =
                        url.Substring(url.LastIndexOf("/") + 1, url.Length - url.LastIndexOf(("/")) - 1).ToLower();
                }

                if (urlNode != null && Template.GetTemplateIdFromAlias(templateAlias) != 0)
                {
                    _redirectID = int.Parse(urlNode.Attributes.GetNamedItem("id").Value);

                    HttpContext.Current.Items["altTemplate"] = templateAlias;
                    HttpContext.Current.Trace.Write("umbraco.altTemplateHandler",
                                                    string.Format("Templated changed to: '{0}'",
                                                                  HttpContext.Current.Items["altTemplate"]));
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
                LogHelper.Info<requestHandler>(string.Format("NotFound url {0} (from '{1}')", url, HttpContext.Current.Request.UrlReferrer));

				// Test if the error404 not child elements
				string error404 = umbraco.library.GetCurrentNotFoundPageId();


				_redirectID = int.Parse(error404);
				HttpContext.Current.Response.StatusCode = 404;
				return true;
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
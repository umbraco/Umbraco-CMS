using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.IO;
using umbraco.presentation;
using umbraco.presentation.LiveEditing;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using System.Xml;
using umbraco.presentation.preview;
using Examine.Providers;
using Examine;

namespace Umbraco.Web
{
    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request
    /// </summary>
    public class UmbracoContext
    {

        private const string HttpContextItemName = "Umbraco.Web.UmbracoContext";
        private static readonly object Locker = new object();

        private PreviewContent _previewContent;

        /// <summary>
        /// Used if not running in a web application (no real HttpContext)
        /// </summary>
        private static UmbracoContext _umbracoContext;

        /// <summary>
        /// Creates a new Umbraco context.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"> </param>
        internal UmbracoContext(HttpContextBase httpContext, ApplicationContext applicationContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");

            HttpContext = httpContext;            
            Application = applicationContext;
        }

        /// <summary>
        /// Gets the current Umbraco Context.
        /// </summary>
        public static UmbracoContext Current
        {
            get
            {
                //if we have a real context then return the request based object
                if (System.Web.HttpContext.Current != null)
                {
                    return (UmbracoContext)System.Web.HttpContext.Current.Items[HttpContextItemName];
                }

                //return the object if not running in a real HttpContext
                return _umbracoContext;
            }

            set
            {
                lock (Locker)
                {
                    //if running in a real HttpContext, this can only be set once
                    if (System.Web.HttpContext.Current != null && Current != null)
                    {
                        throw new ApplicationException("The current httpContext can only be set once during a request.");
                    }

                    //if there is an HttpContext, return the item
                    if (System.Web.HttpContext.Current != null)
                    {
                        System.Web.HttpContext.Current.Items[HttpContextItemName] = value;
                    }
                    else
                    {
                        _umbracoContext = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current ApplicationContext
        /// </summary>
        public ApplicationContext Application { get; private set; }

        /// <summary>
        /// Gets/sets the original URL of the request
        /// </summary>
        internal Uri OriginalUrl { get; set; }

        /// <summary>
        /// Returns the XML Cache document
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is marked internal for now because perhaps we might return a wrapper like CacheData so that it doesn't have a reliance
        /// specifically on XML.
        /// </remarks>
        internal XmlDocument GetXml()
        {
            if (InPreviewMode)
            {
                if (_previewContent == null)
                {
                    _previewContent = new PreviewContent(UmbracoUser, new Guid(StateHelper.Cookies.Preview.GetValue()), true);
                    if (_previewContent.ValidPreviewSet)
                        _previewContent.LoadPreviewset();
                }
                if (_previewContent.ValidPreviewSet)
                    return _previewContent.XmlContent;
            }
            return content.Instance.XmlContent;

        }

        /// <summary>
        /// Gets/sets the DocumentRequest object
        /// </summary>
        internal DocumentRequest DocumentRequest { get; set; }

        /// <summary>
        /// Exposes the HttpContext for the current request
        /// </summary>
        public HttpContextBase HttpContext { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request has debugging enabled
        /// </summary>
        /// <value><c>true</c> if this instance is debug; otherwise, <c>false</c>.</value>
        public bool IsDebug
        {
            get
            {
                return GlobalSettings.DebugMode && (!string.IsNullOrEmpty(HttpContext.Request["umbdebugshowtrace"]) || !string.IsNullOrEmpty(HttpContext.Request["umbdebug"]));
            }
        }

        /// <summary>
        /// Gets the current page ID, or <c>null</c> if no page ID is available (e.g. a custom page).
        /// </summary>
        public virtual int? PageId
        {
            get
            {
                try
                {
                    //TODO: this should be done with a wrapper: http://issues.umbraco.org/issue/U4-61
                    return int.Parse(HttpContext.Items["pageID"].ToString());
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the current logged in Umbraco user (editor).
        /// </summary>
        /// <value>The Umbraco user object or null</value>
        public virtual User UmbracoUser
        {
            get
            {
                return UmbracoEnsuredPage.CurrentUser;
            }

        }

        /// <summary>
        /// Determines whether the current user is in a preview mode and browsing the site (ie. not in the admin UI)
        /// </summary>
        public virtual bool InPreviewMode
        {
            get
            {
                if (HttpContext.Request == null || HttpContext.Request.Url == null)
                    return false;

                var currentUrl = HttpContext.Request.Url.AbsolutePath;
                // zb-00004 #29956 : refactor cookies names & handling
                return
                    StateHelper.Cookies.Preview.HasValue // has preview cookie
                    && UmbracoUser != null // has user
                    && !currentUrl.StartsWith(IOHelper.ResolveUrl(SystemDirectories.Umbraco)); // is not in admin UI
            }
        }   

        /// <summary>
        /// Gets the current Live Editing Context.
        /// </summary>
        public virtual ILiveEditingContext LiveEditingContext
        {
            get
            {
                //TODO: this should be done with a wrapper: http://issues.umbraco.org/issue/U4-61
                var value = (ILiveEditingContext)HttpContext.Items["LiveEditingContext"];
                if (value == null)
                {
                    LiveEditingContext = value = new DefaultLiveEditingContext();
                }
                return value;
            }

            set
            {
                //TODO: this should be done with a wrapper: http://issues.umbraco.org/issue/U4-61
                HttpContext.Items["LiveEditingContext"] = value;
            }
        }

    }
}
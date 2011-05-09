using System;
using System.Web;
using umbraco.presentation.LiveEditing;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using System.Xml.Linq;
using umbraco.BusinessLogic;
using System.Xml;
using umbraco.presentation.preview;
using Examine.Providers;
using Examine;

namespace umbraco.presentation
{
    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request.
    /// </summary>
    public class UmbracoContext
    {
        private UmbracoServerUtility m_Server;
        private UmbracoRequest m_Request;
        private UmbracoResponse m_Response;
        private HttpContext m_HttpContext;
        private XmlDocument previewDocument;
        private PreviewContent _previewContent;

        /// <summary>
        /// Creates a new Umbraco context.
        /// </summary>
        /// <param name="httpContext">The HTTP context on which the Umbraco context operates.</param>
        public UmbracoContext(HttpContext httpContext)
        {
            m_HttpContext = httpContext;
        }

        /// <summary>
        /// Gets the current Umbraco Context.
        /// </summary>
        public static UmbracoContext Current
        {
            get
            {
                return (UmbracoContext)HttpContext.Current.Items["UmbracoContext"];
            }

            set
            {
                if (Current != null)
                    throw new ApplicationException("The current httpContext can only be set once during a request.");

                HttpContext.Current.Items["UmbracoContext"] = value;
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
                    return int.Parse(m_HttpContext.Items["pageID"].ToString());
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
                return BasePages.UmbracoEnsuredPage.CurrentUser;
            }

        }

        /// <summary>
        /// Determines whether the current user is in a preview mode and browsing the site (ie. not in the admin UI)
        /// </summary>
        public virtual bool InPreviewMode
        {
            get
            {
                string currentUrl = Request.Url.AbsolutePath;
				// zb-00004 #29956 : refactor cookies names & handling
				return
					StateHelper.Cookies.Preview.HasValue // has preview cookie
					&& UmbracoUser != null // has user
					&& !currentUrl.StartsWith(IO.IOHelper.ResolveUrl(IO.SystemDirectories.Umbraco)); // is not in admin UI
            }
        }

        public XmlDocument GetXml()
        {
            if (InPreviewMode)
            {
                if (_previewContent == null)
                {
                    _previewContent = new PreviewContent(new Guid(StateHelper.Cookies.Preview.GetValue()));
                    _previewContent.LoadPreviewset();
                }
                return _previewContent.XmlContent;
            }
            else
            {
                return content.Instance.XmlContent;
            }
        }

        /// <summary>
        /// Determines whether the current user has the specified permission on the current page.
        /// </summary>
        /// <param name="permissionToken">The permission token.</param>
        /// <returns>
        /// 	<c>true</c> if the user has permission; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool HasPermission(char permissionToken)
        {
            int? pageId = UmbracoContext.Current.PageId;
            return pageId.HasValue
                    && UmbracoEnsuredPage.CurrentUser != null
                    && UmbracoEnsuredPage.CurrentUser.GetPermissions(new Document(pageId.Value).Path)
                        .Contains(permissionToken.ToString());
        }

        public virtual bool NewSchemaMode
        {
            get
            {
                return !UmbracoSettings.UseLegacyXmlSchema;
            }
        }

        /// <summary>
        /// Gets the current Live Editing Context.
        /// </summary>
        public virtual ILiveEditingContext LiveEditingContext
        {
            get
            {
                ILiveEditingContext value = (ILiveEditingContext)m_HttpContext.Items["LiveEditingContext"];
                if (value == null)
                {
                    LiveEditingContext = value = new DefaultLiveEditingContext();
                }
                return value;
            }

            set
            {
                m_HttpContext.Items["LiveEditingContext"] = value;
            }
        }

        /// <summary>
        /// Gets the response for the current context
        /// </summary>
        /// <value>The response.</value>
        public virtual UmbracoResponse Response
        {
            get
            {
                if (m_Response == null)
                {
                    m_Response = new UmbracoResponse(this.m_HttpContext.Response);
                }
                return m_Response;
            }
        }

        /// <summary>
        /// Gets the request for the current context
        /// </summary>
        /// <value>The request.</value>
        public virtual UmbracoRequest Request
        {
            get
            {
                if (m_Request == null)
                {
                    m_Request = new UmbracoRequest(this.m_HttpContext.Request);
                }
                return m_Request;
            }
        }

        /// <summary>
        /// Gets the base URL for the website
        /// </summary>
        /// <returns></returns>
        public virtual string GetBaseUrl()
        {
            return this.Request.Url.GetLeftPart(UriPartial.Authority);
        }

        public virtual UmbracoServerUtility Server
        {
            get
            {
                if (m_Server == null)
                {
                    m_Server = new UmbracoServerUtility(this.m_HttpContext.Server);
                }
                return m_Server;
            }
        }

        /// <summary>
        /// Gets the internal search provider from Examine.
        /// </summary>
        /// <value>The internal search provider.</value>
        public virtual BaseSearchProvider InternalSearchProvider
        {
            get
            {
                return ExamineManager.Instance.SearchProviderCollection["InternalSearcher"];
            }
        }

        /// <summary>
        /// Gets the internal member search provider from Examine.
        /// </summary>
        /// <value>The internal search provider.</value>
        public virtual BaseSearchProvider InternalMemberSearchProvider
        {
            get
            {
                return ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
            }
        }
    }
}
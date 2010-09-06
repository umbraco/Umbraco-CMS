using System;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using umbraco.presentation;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;

namespace umbraco
{
    /// <summary>
    /// Summary description for WebForm1.
    /// </summary>
    /// 
    public partial class UmbracoDefault : Page
    {

        private Guid m_version = Guid.Empty;
        private string m_tmp = requestHandler.cleanUrl();
        private page m_umbPage = null;
        private requestHandler m_umbRequest = null;

        private bool m_validateRequest = true;

        /// <summary>
        /// To turn off request validation set this to false before the PageLoad event. This equelevant to the validateRequest page directive
        /// and has nothing to do with "normal" validation controls. Default value is true.
        /// </summary>
        public bool ValidateRequest
        {
            get { return m_validateRequest; }
            set { m_validateRequest = value; }
        }

        protected override void Render(HtmlTextWriter output)
        {

            // Get content
            TextWriter tempWriter = new StringWriter();
            base.Render(new HtmlTextWriter(tempWriter));
            string pageContents = tempWriter.ToString();

            pageContents = template.ParseInternalLinks(pageContents);

            // preview
            if (UmbracoContext.Current.InPreviewMode)
            {
                Trace.Write("Runtime Engine", "Umbraco is running in preview mode.");

                int bodyPos = pageContents.ToLower().IndexOf("</body>");
                if (bodyPos > -1)
                {
                    string htmlBadge =
                        String.Format(UmbracoSettings.PreviewBadge, 
                        umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco),
                        umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco_client),
                        Server.UrlEncode(UmbracoContext.Current.Request.Path)
                        );

                    // inject badge
                    pageContents =
                        pageContents.Substring(0, bodyPos) +
                        htmlBadge + pageContents.Substring(bodyPos, pageContents.Length - bodyPos);
                }
            }

            output.Write(pageContents);

        }

        void Page_PreInit(Object sender, EventArgs e)
        {
            Trace.Write("umbracoInit", "handling request");

            if (UmbracoContext.Current == null)
                UmbracoContext.Current = new UmbracoContext(HttpContext.Current);

            bool editMode = UmbracoContext.Current.LiveEditingContext.Enabled;

            if (editMode)
                ValidateRequest = false;

            if (m_tmp != "" && Request["umbPageID"] == null)
            {
                // Check numeric
                string tryIntParse = m_tmp.Replace("/", "").Replace(".aspx", string.Empty);
                int result;
                if (int.TryParse(tryIntParse, out result))
                {
                    m_tmp = m_tmp.Replace(".aspx", string.Empty);

                    // Check for request
                    if (!string.IsNullOrEmpty(Request["umbVersion"]))
                    {
                        // Security check
                        BasePages.UmbracoEnsuredPage bp = new BasePages.UmbracoEnsuredPage();
                        bp.ensureContext();
                        m_version = new Guid(Request["umbVersion"]);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Request["umbPageID"]))
                {
                    int result;
                    if (int.TryParse(Request["umbPageID"], out result))
                    {
                        m_tmp = Request["umbPageID"];
                    }
                }
            }

            if (m_version != Guid.Empty)
            {
                HttpContext.Current.Items["pageID"] = m_tmp.Replace("/", "");
                m_umbPage = new page(int.Parse(m_tmp.Replace("/", "")), m_version);
            }
            else
            {
                m_umbRequest = new requestHandler(UmbracoContext.Current.GetXml(), m_tmp);
                Trace.Write("umbracoInit", "Done handling request");
                if (m_umbRequest.currentPage != null)
                {
                    HttpContext.Current.Items["pageID"] = m_umbRequest.currentPage.Attributes.GetNamedItem("id").Value;

                    // Handle edit
                    if (editMode)
                    {
                        Document d = new Document(int.Parse(m_umbRequest.currentPage.Attributes.GetNamedItem("id").Value));
                        m_umbPage = new page(d.Id, d.Version);
                    }
                    else
                        m_umbPage = new page(m_umbRequest.currentPage);
                }
            }

            // set the friendly path so it's used by forms
            HttpContext.Current.RewritePath(HttpContext.Current.Items[requestModule.ORIGINAL_URL_CXT_KEY].ToString());

            if (UmbracoSettings.UseAspNetMasterPages)
            {
                HttpContext.Current.Trace.Write("umbracoPage", "Looking up skin information");

                if (m_umbPage != null)
                    this.MasterPageFile = template.GetMasterPageName(m_umbPage.Template);

                initUmbracoPage();
            }
        }

        public Control pageContent = new Control();

        protected void Page_Load(object sender, EventArgs e)
        {

            if (ValidateRequest)
                Request.ValidateInput();

            if (!String.IsNullOrEmpty(Request["umbDebugShowTrace"]))
            {
                if (!GlobalSettings.DebugMode)
                {
                    Page.Trace.IsEnabled = false;
                }
            }
            else
                Page.Trace.IsEnabled = false;
        }


        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();

            if (!UmbracoSettings.UseAspNetMasterPages)
                initUmbracoPage();
            base.OnInit(e);
        }

        private void initUmbracoPage()
        {

            RequestInitEventArgs e = new RequestInitEventArgs();
            e.Page = m_umbPage;
            e.PageId = m_umbPage.PageID;
            e.Context = System.Web.HttpContext.Current;
            
            FireBeforeRequestInit(e);
            if (!e.Cancel)
            {


                if (!UmbracoSettings.EnableSplashWhileLoading || !content.Instance.isInitializing)
                {

                    if (m_umbPage != null)
                    {
                        // Add page elements to global items
                        try
                        {

                            System.Web.HttpContext.Current.Items.Add("pageElements", m_umbPage.Elements);

                        }
                        catch (ArgumentException aex)
                        {

                            System.Web.HttpContext.Current.Items.Remove("pageElements");
                            System.Web.HttpContext.Current.Items.Add("pageElements", m_umbPage.Elements);
                        }

                        string tempCulture = m_umbPage.GetCulture();
                        if (tempCulture != "")
                        {
                            System.Web.HttpContext.Current.Trace.Write("default.aspx", "Culture changed to " + tempCulture);
                            System.Threading.Thread.CurrentThread.CurrentCulture =
                                System.Globalization.CultureInfo.CreateSpecificCulture(tempCulture);
                            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                        }

                        if (!UmbracoSettings.UseAspNetMasterPages)
                        {
                            layoutControls.umbracoPageHolder pageHolder = new umbraco.layoutControls.umbracoPageHolder();
                            pageHolder.ID = "umbPageHolder";
                            Page.Controls.Add(pageHolder);
                            m_umbPage.RenderPage(m_umbPage.Template);
                            layoutControls.umbracoPageHolder umbPageHolder =
                                (layoutControls.umbracoPageHolder)Page.FindControl("umbPageHolder");
                            umbPageHolder.Populate(m_umbPage);
                        }

                    }
                    else
                    {
                        // If there's no published content, show friendly error
                        if (umbraco.content.Instance.XmlContent.SelectSingleNode("/root/*") == null)
                            Response.Redirect(IO.SystemDirectories.Config + "/splashes/noNodes.aspx");
                        else
                        {

                            Response.StatusCode = 404;
                            Response.Write("<html><body><h1>Page not found</h1>");
                            if (m_umbRequest != null)
                                HttpContext.Current.Response.Write("<h3>No umbraco document matches the url '" + HttpUtility.HtmlEncode(Request.Url.ToString()) + "'</h3><p>umbraco tried this to match it using this xpath query'" + m_umbRequest.PageXPathQuery + "')");
                            else
                                HttpContext.Current.Response.Write("<h3>No umbraco document matches the url '" + HttpUtility.HtmlEncode(Request.Url.ToString()) + "'</h3>");
                            Response.Write("</p>");
                            Response.Write("<p>This page can be replaced with a custom 404 page by adding the id of the umbraco document to show as 404 page in the /config/umbracoSettings.config file. Just add the id to the '/settings/content/errors/error404' element.</p>");
                            Response.Write("<p>For more information, visit <a href=\"http://umbraco.org/redir/custom-404\">information about custom 404</a> on the umbraco website.</p>");
                            Response.Write("<p style=\"border-top: 1px solid #ccc; padding-top: 10px\"><small>This page is intentionally left ugly ;-)</small></p>");
                            Response.Write("</body></html>");
                        }
                    }
                }
                else
                {
                    Response.Redirect(IO.SystemDirectories.Config + "/splashes/booting.aspx?orgUrl=" + Request.Url);
                }

                FireAfterRequestInit(e);
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion



        /// <summary>
        /// The preinit event handler
        /// </summary>
        public delegate void RequestInitEventHandler(object sender, RequestInitEventArgs e);
        /// <summary>
        /// occurs before the umbraco page is initialized for rendering.
        /// </summary>
        public static event RequestInitEventHandler BeforeRequestInit;
        /// <summary>
        /// Raises the <see cref="E:BeforeRequestInit"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected internal new virtual void FireBeforeRequestInit(RequestInitEventArgs e)
        {
            if (BeforeRequestInit != null)
                BeforeRequestInit(this, e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public static event RequestInitEventHandler AfterRequestInit;
        /// <summary>
        /// Raises the <see cref="E:AfterRequestInit"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterRequestInit(RequestInitEventArgs e)
        {
            if (AfterRequestInit != null)
                AfterRequestInit(this, e);
            
        }        
    }

        //Request Init Events Arguments
        public class RequestInitEventArgs : System.ComponentModel.CancelEventArgs
        {
            public page Page { get; internal set; }
            public HttpContext Context { get; internal set; }
            public string Url { get; internal set; }
            public int PageId { get; internal set; }
        }
}

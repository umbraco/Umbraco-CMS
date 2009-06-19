using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using umbraco.uicontrols;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for dashboard.
    /// </summary>
    public partial class dashboard : BasePages.UmbracoEnsuredPage
    {


        private string _section = "";

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Put user code to initialize the page here
            Panel2.Text = ui.Text("dashboard", "welcome", base.getUser()) + " " + this.getUser().Name;
        }

        private Control CreateDashBoardWrapperControl(Control control)
        {
            PlaceHolder placeHolder = new PlaceHolder();
            placeHolder.Controls.Add(new LiteralControl("<br/><fieldSet style=\"padding: 5px\">"));
            placeHolder.Controls.Add(control);
            placeHolder.Controls.Add(new LiteralControl("</fieldSet>"));
            return placeHolder;
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
            // Load dashboard content
            if (helper.Request("app") != "")
                _section = helper.Request("app");
            else if (getUser().Applications.Length > 0)
                _section = "default";
            else
                _section = getUser().Applications[0].alias;

            XmlDocument dashBoardXml = new XmlDocument();
            dashBoardXml.Load(HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../config/dashboard.config"));

            // test for new tab interface
            XmlNode section = dashBoardXml.DocumentElement.SelectSingleNode("./section [areas/area = '" + _section.ToLower() + "']");
            if (section != null)
            {
                Panel2.Visible = false;
                dashboardTabs.Visible = true;
                //bodyAttributes.Text = " onresize=\"resizeTabView(dashboardTabs_tabs, 'dashboardTabs')\" onload=\"resizeTabView(dashboardTabs_tabs, 'dashboardTabs')\"";
                foreach (XmlNode entry in section.SelectNodes("./tab"))
                {
                    TabPage tab = dashboardTabs.NewTabPage(entry.Attributes.GetNamedItem("caption").Value);
                    tab.HasMenu = true;
                    tab.Style.Add("padding", "0 10px");

                    foreach (XmlNode uc in entry.SelectNodes("./control"))
                        try
                        {
                            tab.Controls.Add(LoadControl(uc.FirstChild.Value));
                        }
                        catch (Exception ee)
                        {
                            tab.Controls.Add(new LiteralControl("<p class=\"umbracoErrorMessage\">Could not load control '" + uc.FirstChild.Value + "'. <br/><span class=\"guiDialogTiny\"><strong>Error message:</strong> " + ee.ToString() + "</span></p>"));
                        }
                }

            }
            else
            {
                //bodyAttributes.Text = " onLoad=\"resizePanel('Panel2',false);\" onResize=\"resizePanel('Panel2',false);\"";

                foreach (XmlNode entry in dashBoardXml.SelectNodes("//entry [@section='" + _section.ToLower() + "']"))
                {
                    PlaceHolder placeHolder = new PlaceHolder();
                    if (entry == null || entry.FirstChild == null)
                    {
                        placeHolder.Controls.Add(CreateDashBoardWrapperControl(new LiteralControl("Error loading DashBoard Content")));
                    }
                    else
                    {
                        try
                        {
                            placeHolder.Controls.Add(CreateDashBoardWrapperControl(LoadControl(entry.FirstChild.Value)));
                        }
                        catch (Exception err)
                        {
                            Trace.Warn("Dashboard", string.Format("error loading control '{0}'",
                                entry.FirstChild.Value), err);
                            placeHolder.Controls.Clear();
                            placeHolder.Controls.Add(CreateDashBoardWrapperControl(new LiteralControl(string.Format(
                                "Error loading DashBoard Content '{0}'; {1}", entry.FirstChild.Value,
                                err.Message))));
                        }
                    }
                    dashBoardContent.Controls.Add(placeHolder);
                }
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
    }
}

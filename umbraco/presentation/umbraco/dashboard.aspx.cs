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
using umbraco.IO;
using umbraco.cms.helpers;
using umbraco.BusinessLogic;

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
            dashBoardXml.Load(IOHelper.MapPath(SystemFiles.DashboardConfig));

            // test for new tab interface
            XmlNode section = dashBoardXml.DocumentElement.SelectSingleNode("./section [areas/area = '" + _section.ToLower() + "']");
            if (section != null && validateAccess(section))
            {
                Panel2.Visible = false;
                dashboardTabs.Visible = true;

                foreach (XmlNode entry in section.SelectNodes("./tab"))
                {
                    if (validateAccess(entry))
                    {
                        TabPage tab = dashboardTabs.NewTabPage(entry.Attributes.GetNamedItem("caption").Value);
                        tab.HasMenu = true;
                        tab.Style.Add("padding", "0 10px");

                        foreach (XmlNode uc in entry.SelectNodes("./control"))
                        {
                            if (validateAccess(uc))
                            {
                                string control = getFirstText(uc).Trim(' ', '\r', '\n');
                                string path = IOHelper.FindFile(control);
                                

                                try
                                {
                                    Control c = LoadControl(path);
                                
                                    //resolving files from dashboard config which probably does not map to a virtual fi
                                    tab.Controls.Add(AddShowOnceLink(uc));
                                    tab.Controls.Add(c);
                                }
                                catch (Exception ee)
                                {
                                    tab.Controls.Add(new LiteralControl("<p class=\"umbracoErrorMessage\">Could not load control: '" + path + "'. <br/><span class=\"guiDialogTiny\"><strong>Error message:</strong> " + ee.ToString() + "</span></p>"));
                                }
                            }
                        }
                    }
                }

            }
            else
            {


                foreach (XmlNode entry in dashBoardXml.SelectNodes("//entry [@section='" + _section.ToLower() + "']"))
                {
                    PlaceHolder placeHolder = new PlaceHolder();
                    if (entry == null || entry.FirstChild == null)
                    {
                        placeHolder.Controls.Add(CreateDashBoardWrapperControl(new LiteralControl("Error loading DashBoard Content")));
                    }
                    else
                    {
                        string path = IOHelper.FindFile(entry.FirstChild.Value);

                        try
                        {
                            placeHolder.Controls.Add(CreateDashBoardWrapperControl(LoadControl(path)));
                        }
                        catch (Exception err)
                        {
                            Trace.Warn("Dashboard", string.Format("error loading control '{0}'",
                                path), err);
                            placeHolder.Controls.Clear();
                            placeHolder.Controls.Add(CreateDashBoardWrapperControl(new LiteralControl(string.Format(
                                "Error loading DashBoard Content '{0}'; {1}", path,
                                err.Message))));
                        }
                    }
                    dashBoardContent.Controls.Add(placeHolder);
                }
            }
        }

        private LiteralControl AddShowOnceLink(XmlNode node)
        {
            LiteralControl onceLink = new LiteralControl();
            if (node.Attributes.GetNamedItem("showOnce") != null &&
                node.Attributes.GetNamedItem("showOnce").Value.ToLower() == "true")
            {
                onceLink.Text = "<a class=\"dashboardHideLink\" href=\"javascript:jQuery.cookie('" + generateCookieKey(node) + "','true');\">" + ui.Text("dashboard", "dontShowAgain") + "</a>";
            }
            return onceLink;
        }

        private string getFirstText(XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Text)
                    return n.Value;
            }

            return "";
        }

        private string generateCookieKey(XmlNode node)
        {
            string key = String.Empty;
            if (node.Name.ToLower() == "control")
            {
                key = node.FirstChild.Value + "_" + generateCookieKey(node.ParentNode);
            }
            else if (node.Name.ToLower() == "tab")
            {
                key = node.Attributes.GetNamedItem("caption").Value;
            }

            return Casing.SafeAlias(key.ToLower());
        }

        private bool validateAccess(XmlNode node)
        {
            // check if this area should be shown at all
            string onlyOnceValue = StateHelper.GetCookieValue(generateCookieKey(node));
            if (!String.IsNullOrEmpty(onlyOnceValue))
            {
                return false;
            }

            // the root user can always see everything
            if (CurrentUser.IsRoot())
            {
                return true;
            }
            else if (node != null)
            {
                XmlNode accessRules = node.SelectSingleNode("access");
                bool retVal = true;
                if (accessRules != null && accessRules.HasChildNodes)
                {

                    string currentUserType = CurrentUser.UserType.Alias.ToLower();
                    string allowedSections = ",";
                    foreach (BusinessLogic.Application app in CurrentUser.Applications)
                    {
                        allowedSections += app.alias.ToLower() + ",";
                    }
                    XmlNodeList grantedTypes = accessRules.SelectNodes("grant");
                    XmlNodeList grantedBySectionTypes = accessRules.SelectNodes("grantBySection");
                    XmlNodeList deniedTypes = accessRules.SelectNodes("deny");

                    // if there's a grant type, everyone who's not granted is automatically denied
                    if (grantedTypes.Count > 0 || grantedBySectionTypes.Count > 0)
                    {
                        retVal = false;
                        if (grantedBySectionTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("grantBySection [contains('{0}', concat(',',.,','))]", allowedSections)) != null)
                        {
                            retVal = true;
                        }
                        else if (grantedTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("grant [. = '{0}']", currentUserType)) != null)
                        {
                            retVal = true;
                        }
                    }
                    // if the current type of user is denied we'll say nay
                    if (deniedTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("deny [. = '{0}']", currentUserType)) != null)
                    {
                        retVal = false;
                    }

                }

                return retVal;
            }
            return false;
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

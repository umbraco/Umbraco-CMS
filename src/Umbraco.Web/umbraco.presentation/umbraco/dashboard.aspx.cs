using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;
using umbraco.uicontrols;
using Umbraco.Core.IO;
using umbraco.cms.helpers;
using umbraco.BusinessLogic;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for dashboard.
    /// </summary>
    public partial class dashboard : BasePages.UmbracoEnsuredPage
    {
        private string _section;

        protected string Section
        {
            get
            {
                if (_section == null)
                {
                    var qry = Request.CleanForXss("app");
                    // Load dashboard content
                    if (qry.IsNullOrWhiteSpace() == false)
                    {
                        //validate the app
                        if (BusinessLogic.Application.getAll().Any(x => x.alias.InvariantEquals(qry)) == false)
                        {
                            LogHelper.Warn<dashboard>("A requested app: " + Request.GetItemAsString("app") + " was not found");
                            _section = "default";
                        }
                        else
                        {
                            _section = qry;
                        }
                    }
                    else if (UmbracoUser.Applications.Length > 0)
                    {
                        _section = "default";
                    }
                    else
                    {
                        _section = UmbracoUser.Applications[0].alias;
                    }
                }
                return _section;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Put user code to initialize the page here
            Panel2.Text = ui.Text("dashboard", "welcome", UmbracoUser) + " " + UmbracoUser.Name;
        }

        private static Control CreateDashBoardWrapperControl(Control control)
        {
            var placeHolder = new PlaceHolder();
            placeHolder.Controls.Add(new LiteralControl("<br/><fieldSet style=\"padding: 5px\">"));
            placeHolder.Controls.Add(control);
            placeHolder.Controls.Add(new LiteralControl("</fieldSet>"));
            return placeHolder;
        }

        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);
           
            var dashBoardXml = new XmlDocument();
            dashBoardXml.Load(IOHelper.MapPath(SystemFiles.DashboardConfig));

            // test for new tab interface
            if (dashBoardXml.DocumentElement == null) return;
            var nodeList = dashBoardXml.DocumentElement.SelectNodes("//section [areas/area = '" + Section.ToLower() + "']");
            if (nodeList == null) return;

            foreach (XmlNode section in nodeList)
            {
                if (section != null && ValidateAccess(section))
                {
                    Panel2.Visible = false;
                    dashboardTabs.Visible = true;

                    var xmlNodeList = section.SelectNodes("./tab");
                    if (xmlNodeList != null)
                    {
                        foreach (XmlNode entry in xmlNodeList)
                        {
                            if (ValidateAccess(entry) == false) continue;
                            if (entry.Attributes == null) continue;
                            var tab = dashboardTabs.NewTabPage(entry.Attributes.GetNamedItem("caption").Value);
                            tab.HasMenu = true;
                            tab.Style.Add("padding", "0 10px");

                            var selectNodes = entry.SelectNodes("./control");
                            if (selectNodes == null) continue;

                            foreach (XmlNode uc in selectNodes)
                            {
                                if (ValidateAccess(uc) == false) continue;

                                var control = GetFirstText(uc).Trim(' ', '\r', '\n');
                                var path = IOHelper.FindFile(control);

                                try
                                {
                                    var c = LoadControl(path);

                                    // set properties
                                    var type = c.GetType();
                                    if (uc.Attributes != null)
                                    {
                                        foreach (XmlAttribute att in uc.Attributes)
                                        {
                                            var attributeName = att.Name;
                                            var attributeValue = ParseControlValues(att.Value).ToString();
                                            // parse special type of values

                                            var prop = type.GetProperty(attributeName);
                                            if (prop == null)
                                            {
                                                continue;
                                            }

                                            prop.SetValue(c, Convert.ChangeType(attributeValue, prop.PropertyType), null);

                                        }
                                    }
                                    //resolving files from dashboard config which probably does not map to a virtual fi
                                    tab.Controls.Add(AddPanel(uc, c));
                                }
                                catch (Exception ee)
                                {
                                    tab.Controls.Add(
                                        new LiteralControl(
                                            "<p class=\"umbracoErrorMessage\">Could not load control: '" + path +
                                            "'. <br/><span class=\"guiDialogTiny\"><strong>Error message:</strong> " +
                                            ee + "</span></p>"));
                                }
                            }
                        }
                    }
                }
                else
                {
                    var xmlNodeList = dashBoardXml.SelectNodes("//entry [@section='" + Section.ToLower() + "']");
                    if (xmlNodeList != null)
                    {
                        foreach (XmlNode entry in xmlNodeList)
                        {
                            var placeHolder = new PlaceHolder();
                            if (entry == null || entry.FirstChild == null)
                            {
                                placeHolder.Controls.Add(
                                    CreateDashBoardWrapperControl(new LiteralControl("Error loading DashBoard Content")));
                            }
                            else
                            {
                                var path = IOHelper.FindFile(entry.FirstChild.Value);

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
            }
        }

        private static object ParseControlValues(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                if (value.StartsWith("[#"))
                {
                    value = value.Substring(2, value.Length - 3).ToLower();
                    switch (value)
                    {
                        case "usertype":
                            return BusinessLogic.User.GetCurrent().UserType.Alias;
                        case "username":
                            return BusinessLogic.User.GetCurrent().Name;
                        case "userlogin":
                            return BusinessLogic.User.GetCurrent().LoginName;
                        case "usercontentstartnode":
                            return BusinessLogic.User.GetCurrent().StartNodeId;
                        case "usermediastartnode":
                            return BusinessLogic.User.GetCurrent().StartMediaId;
                        default:
                            return value;
                    }
                }
            }

            return value;
        }

        private static Control AddPanel(XmlNode node, Control c)
        {
            var hide = AddShowOnceLink(node);
            if (node.Attributes.GetNamedItem("addPanel") != null &&
                node.Attributes.GetNamedItem("addPanel").Value == "true")
            {
                var p = new Pane();
                var pp = new PropertyPanel();
                if (node.Attributes.GetNamedItem("panelCaption") != null &&
                    string.IsNullOrEmpty(node.Attributes.GetNamedItem("panelCaption").Value) == false)
                {
                    var panelCaption = node.Attributes.GetNamedItem("panelCaption").Value;
                    if (panelCaption.StartsWith("#"))
                    {
                        panelCaption = ui.Text(panelCaption.Substring(1));
                    }
                    pp.Text = panelCaption;
                }
                // check for hide in the future link
                if (string.IsNullOrEmpty(hide.Text) == false)
                {
                    pp.Controls.Add(hide);
                }
                pp.Controls.Add(c);
                p.Controls.Add(pp);
                return p;
            }

            if (string.IsNullOrEmpty(hide.Text) == false)
            {
                var ph = new PlaceHolder();
                ph.Controls.Add(hide);
                ph.Controls.Add(c);
                return ph;
            }
            return c;
        }

        private static LiteralControl AddShowOnceLink(XmlNode node)
        {
            var onceLink = new LiteralControl();
            if (node.Attributes.GetNamedItem("showOnce") != null &&
                node.Attributes.GetNamedItem("showOnce").Value.ToLower() == "true")
            {
                onceLink.Text = "<a class=\"dashboardHideLink\" onclick=\"if(confirm('Are you sure you want remove this dashboard item?')){jQuery.cookie('" + GenerateCookieKey(node) + "','true'); jQuery(this).closest('.propertypane').fadeOut();return false;}\">" + ui.Text("dashboard", "dontShowAgain") + "</a>";
            }
            return onceLink;
        }

        private static string GetFirstText(XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Text)
                {
                    return n.Value;
                }
            }

            return "";
        }

        private static string GenerateCookieKey(XmlNode node)
        {
            var key = String.Empty;
            if (node.Name.ToLower() == "control")
            {
                key = node.FirstChild.Value + "_" + GenerateCookieKey(node.ParentNode);
            }
            else if (node.Name.ToLower() == "tab")
            {
                key = node.Attributes.GetNamedItem("caption").Value;
            }

            return Casing.SafeAlias(key.ToLower());
        }

        private static bool ValidateAccess(XmlNode node)
        {
            // check if this area should be shown at all
            var onlyOnceValue = StateHelper.GetCookieValue(GenerateCookieKey(node));
            if (string.IsNullOrEmpty(onlyOnceValue) == false)
            {
                return false;
            }

            // the root user can always see everything
            if (CurrentUser.IsRoot())
            {
                return true;
            }
            if (node != null)
            {
                var accessRules = node.SelectSingleNode("access");
                var retVal = true;
                if (accessRules != null && accessRules.HasChildNodes)
                {
                    var currentUserType = CurrentUser.UserType.Alias.ToLowerInvariant();
                    
                    //Update access rules so we'll be comparing lower case to lower case always

                    var denies = accessRules.SelectNodes("deny");
                    foreach (XmlNode deny in denies)
                    {
                        deny.InnerText = deny.InnerText.ToLowerInvariant();
                    }

                    var grants = accessRules.SelectNodes("grant");
                    foreach (XmlNode grant in grants)
                    {
                        grant.InnerText = grant.InnerText.ToLowerInvariant();
                    }

                    var allowedSections = ",";
                    foreach (Application app in CurrentUser.Applications)
                    {
                        allowedSections += app.alias.ToLower() + ",";
                    }
                    var grantedTypes = accessRules.SelectNodes("grant");
                    var grantedBySectionTypes = accessRules.SelectNodes("grantBySection");
                    var deniedTypes = accessRules.SelectNodes("deny");

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
        /// Panel2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.UmbracoPanel Panel2;

        /// <summary>
        /// dashBoardContent control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder dashBoardContent;

        /// <summary>
        /// dashboardTabs control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.TabView dashboardTabs;

    }
}

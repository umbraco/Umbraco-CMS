using System;
using System.Web.UI;
using ClientDependency.Core;
using ClientDependency.Core.Controls;
using umbraco.interfaces;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.cms.businesslogic;
namespace umbraco.uicontrols.TreePicker
{

    /*
    [ClientDependency(0, ClientDependencyType.Javascript, "Application/NamespaceManager.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "modal/style.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "modal/modal.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "Application/UmbracoClientManager.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "Application/UmbracoUtils.js", "UmbracoClient")]*/

    [ValidationProperty("Value")]
    public abstract class BaseTreePicker : Control, INamingContainer
    {
        
        protected HiddenField ItemIdValue;
        protected HtmlAnchor DeleteLink;
        protected HtmlAnchor ChooseLink;
        protected HtmlGenericControl ItemTitle;
        protected HtmlGenericControl ButtonContainer;
        protected HtmlGenericControl RootContainer;

        public BaseTreePicker()
        {
            ShowDelete = true;
            ModalHeight = 400;
            ModalWidth = 300;
            ShowHeader = true;
        }

        /// <summary>
        /// Wraps the hidden vield value
        /// </summary>
        public string Value
        {
            get
            {
                EnsureChildControls();
                return ItemIdValue.Value;
            }
            set
            {
                EnsureChildControls();
                ItemIdValue.Value = value;
            }
        }

        public int ModalWidth { get; set; }
        public int ModalHeight { get; set; }
        public bool ShowDelete { get; set; }
        public bool ShowHeader { get; set; }

        /// <summary>
        /// Need to specify the tree picker url (iframe)
        /// </summary>
        public abstract string TreePickerUrl { get; }

        /// <summary>
        /// The title to specify for the picker window
        /// </summary>
        public abstract string ModalWindowTitle { get; }

        /// <summary>
        /// If item has been selected or stored, this will query the db for its title
        /// </summary>
        protected virtual string GetItemTitle()
        {
            if (!string.IsNullOrEmpty(ItemIdValue.Value))
            {
                try
                {
                    return new CMSNode(int.Parse(ItemIdValue.Value)).Text;
                }
                catch (ArgumentException) { /*the node does not exist! we will ignore*/ }
            }
            return "";
        }

        /// <summary>
        /// Just like GetItemTitle, except returns the full path (breadcrumbs) of the node
        /// </summary>
        protected virtual string GetItemBreadcrumbs()
        {
            if (!string.IsNullOrEmpty(ItemIdValue.Value))
            {
                try
                {
                    int nodeId = -1;
                    if (int.TryParse(ItemIdValue.Value, out nodeId))
                    {
                        CMSNode node = new CMSNode(nodeId);
                        string title = node.Text;
                        string separator = " > ";
                        while (node != null && node.Level > 1)
                        {
                            node = node.Parent;
                            title = node.Text + separator + title;
                        }
                        return title;
                    }
                    else
                    {
                        return ItemIdValue.Value;
                    }
                    
                }
                catch (ArgumentException) { /*the node does not exist! we will ignore*/ }
            }
            return "";
        }

        /// <summary>
        /// Outputs the JavaScript instances used to make this control work
        /// </summary>
        protected virtual string GetJSScript()
        {            
            /* 0 = this control's client id
             * 1 = label 
             * 2 = itemIdValueClientID
             * 3 = itemTitleClientID
             * 4 = itemPickerUrl
             * 5 = popup width
             * 6 = popup height
             * 7 = show header
             * 8 = umbraco path
            */
            return string.Format(@"
                var mc_{0} = new Umbraco.Controls.TreePicker('{0}','{1}','{2}','{3}','{4}',{5},{6},{7},'{8}');",
                    new string[]
                 {
                    this.ClientID,
                    ModalWindowTitle,
                    ItemIdValue.ClientID,                    
                    ItemTitle.ClientID,
                    TreePickerUrl,
                    ModalWidth.ToString(),
                    ModalHeight.ToString(),
                    ShowHeader.ToString().ToLower(),
                    Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco).TrimEnd('/')
                 });
        }

        /// <summary>
        /// Registers the required JS classes required to make this control work
        /// </summary>
        protected virtual void RenderJSComponents()
        {
            const string scriptKey = "BaseTreePickerScripts";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), scriptKey, BaseTreePickerScripts.BaseTreePicker, true);
            }
            else
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(BaseTreePicker), scriptKey, BaseTreePickerScripts.BaseTreePicker, true);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureChildControls();

            //disable view state for this control
            this.EnableViewState = false;
        }

        /// <summary>
        /// Create the native .net child controls for this control
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            RootContainer = new HtmlGenericControl("span");
            RootContainer.Attributes.Add("class", "umb-tree-picker");
            this.Controls.Add(RootContainer);

            //create the hidden field
            ItemIdValue = new HiddenField();
            ItemIdValue.ID = "ContentIdValue";
            RootContainer.Controls.Add(ItemIdValue);

            
            ButtonContainer = new HtmlGenericControl("span");
            ButtonContainer.ID = "btns";

            //add item title with padding
            ItemTitle = new HtmlGenericControl("span");
            ItemTitle.ID = "title";
            ItemTitle.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
            ItemTitle.Attributes.Add("class", "treePickerTitle");      // solely for styling, e.g. with an underline or dotted border, etc.
            ButtonContainer.Controls.Add(ItemTitle);
            ButtonContainer.Attributes.Add("class", "buttons");
            ButtonContainer.Controls.Add(new LiteralControl("&nbsp;"));
            ButtonContainer.Controls.Add(new LiteralControl("&nbsp;"));

            //add the delete link with padding                
            DeleteLink = new HtmlAnchor();
            DeleteLink.HRef = "#"; //set on pre-render
            DeleteLink.Style.Add(HtmlTextWriterStyle.Color, "red");
            DeleteLink.Title = ui.GetText("delete");
            DeleteLink.InnerText = ui.GetText("delete");
            DeleteLink.Attributes.Add("class", "clear");

            ButtonContainer.Controls.Add(DeleteLink);
            ButtonContainer.Controls.Add(new LiteralControl("&nbsp;"));
            ButtonContainer.Controls.Add(new LiteralControl("&nbsp;"));
            if (!ShowDelete)
            {
                DeleteLink.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            RootContainer.Controls.Add(ButtonContainer);

            //add choose link with padding
            ChooseLink = new HtmlAnchor();
            ChooseLink.HRef = "#"; //filled in on pre-render
            ChooseLink.InnerText = ui.GetText("choose") + "...";
            ChooseLink.Attributes.Add("data-section", this.TreePickerUrl);
            ChooseLink.Attributes.Add("class", "choose");

            RootContainer.Controls.Add(ChooseLink);
        }

        /// <summary>
        /// Registers the JavaScript required for the control to function and hides/shows controls depending on it's properties
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //hide the buttons if no item, otherwise get the item title
            if (string.IsNullOrEmpty(ItemIdValue.Value))
            {
                ButtonContainer.Style.Add(HtmlTextWriterStyle.Display, "none");
            }
            else
            {
                ItemTitle.InnerText = GetItemTitle();
                ItemTitle.Attributes.Add("title", GetItemBreadcrumbs());   // Adding full path/meta info (Issue U4-192)
            }  
            /*
            ChooseLink.HRef = string.Format("javascript:mc_{0}.LaunchPicker();", this.ClientID);
            DeleteLink.HRef = string.Format("javascript:mc_{0}.ClearSelection();", this.ClientID);
            */

            RenderJSComponents();

            /*
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), this.ClientID + "TreePicker", GetJSScript(), true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "TreePicker", GetJSScript(), true);
            }*/

        }

        
    }
}

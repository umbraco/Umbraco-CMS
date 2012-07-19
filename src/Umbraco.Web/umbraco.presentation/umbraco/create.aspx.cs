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

using System.Xml.XPath;
using System.Xml;
using umbraco.IO;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for create.
    /// </summary>
    public partial class Create : BasePages.UmbracoEnsuredPage
    {
        protected umbWindow createWindow;
        protected System.Web.UI.WebControls.Label helpText;
        protected System.Web.UI.WebControls.TextBox rename;
        protected System.Web.UI.WebControls.Label Label1;
        protected System.Web.UI.WebControls.ListBox nodeType;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Load create definitions
            string nodeType = Request.QueryString["nodeType"];

            XmlDocument createDef = new XmlDocument();
            XmlTextReader defReader = new XmlTextReader(IOHelper.MapPath(SystemFiles.CreateUiXml));
            createDef.Load(defReader);
            defReader.Close();

            // Find definition for current nodeType
            XmlNode def = createDef.SelectSingleNode("//nodeType [@alias = '" + nodeType + "']");
            if (def == null)
            {
                throw new ArgumentException("The create dialog for \"" + nodeType + "\" does not match anything defined in the \"" + SystemFiles.CreateUiXml + "\". This could mean an incorrectly installed package or a corrupt UI file");
            }
            //title.Text = ui.Text("create") + " " + ui.Text(def.SelectSingleNode("./header").FirstChild.Value.ToLower(), base.getUser());
            try
            {
                //headerTitle.Text = title.Text;
                UI.Controls.Add(new UserControl().LoadControl(SystemDirectories.Umbraco + def.SelectSingleNode("./usercontrol").FirstChild.Value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ERROR CREATING CONTROL FOR NODETYPE: " + nodeType, ex);
            }
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
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

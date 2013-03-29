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
using Umbraco.Core.IO;

namespace umbraco.cms.presentation
{

    public class Create : BasePages.UmbracoEnsuredPage
    {
        protected umbWindow createWindow;
        protected Label helpText;
        protected TextBox rename;
        protected Label Label1;
        protected ListBox nodeType;
        protected PlaceHolder UI;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Load create definitions
            var nodeType = Request.QueryString["nodeType"];

            var createDef = new XmlDocument();
            var defReader = new XmlTextReader(IOHelper.MapPath(SystemFiles.CreateUiXml));
            createDef.Load(defReader);
            defReader.Close();

            // Find definition for current nodeType
            var def = createDef.SelectSingleNode("//nodeType [@alias = '" + nodeType + "']");
            if (def == null)
            {
                throw new ArgumentException("The create dialog for \"" + nodeType + "\" does not match anything defined in the \"" + SystemFiles.CreateUiXml + "\". This could mean an incorrectly installed package or a corrupt UI file");
            }
            //title.Text = ui.Text("create") + " " + ui.Text(def.SelectSingleNode("./header").FirstChild.Value.ToLower(), base.getUser());
            try
            {
                //headerTitle.Text = title.Text;
                UI.Controls.Add(LoadControl(SystemDirectories.Umbraco + def.SelectSingleNode("./usercontrol").FirstChild.Value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ERROR CREATING CONTROL FOR NODETYPE: " + nodeType, ex);
            }
        }


    }
}

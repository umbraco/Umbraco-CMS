using System;
using System.Web.UI.WebControls;

using System.Xml;
using Umbraco.Core.IO;

namespace umbraco.cms.presentation
{
    public class Create : BasePages.UmbracoEnsuredPage
    {
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

            try
            {
                var virtualPath = SystemDirectories.Umbraco + def.SelectSingleNode("./usercontrol").FirstChild.Value;
                var mainControl = LoadControl(virtualPath);
                UI.Controls.Add(mainControl);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ERROR CREATING CONTROL FOR NODETYPE: " + nodeType, ex);
            }
        }
    }
}

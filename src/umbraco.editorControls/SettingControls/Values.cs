using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.cms.businesslogic.datatype;

[assembly: System.Web.UI.WebResource("umbraco.editorControls.SettingControls.js.Values.js", "text/js")]
[assembly: System.Web.UI.WebResource("umbraco.editorControls.SettingControls.css.Values.css", "text/css")]
namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class Values : DataEditorSettingType
    {

        private Panel p = new Panel();
        private TextBox tb = new TextBox();


        public override string Value
        {
            get
            {
                return tb.Text;
            }
            set
            {
                tb.Text = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {
            tb.ID = sender.GetName();
            tb.CssClass = "valuesInput";
            tb.Attributes.Add("style", "display:none;");


            string html = "<div class='values'>";

            html += "</div>";

            html += "<input type='text' class='valueInput' />";
            html += " <a onclick=\"valuesDataEditorSettingTypeAddValue(this); return false;\" href='#' class='Add'>Add Value</a>";


            p.Controls.Add(new System.Web.UI.LiteralControl("<div class='valuesDataEditorSettingType'>"));
            p.Controls.Add(new System.Web.UI.LiteralControl(html));
            p.Controls.Add(tb);
            p.Controls.Add(new System.Web.UI.LiteralControl("</div>"));
          

            System.Web.UI.Page page = (System.Web.UI.Page)HttpContext.Current.Handler;


            page.ClientScript.RegisterClientScriptInclude(
                "umbraco.editorControls.SettingControls.js.Values.js",
                page.ClientScript.GetWebResourceUrl(typeof(Values), "umbraco.editorControls.SettingControls.js.Values.js"));


            HtmlHead head = (HtmlHead)page.Header;
            HtmlLink link = new HtmlLink();
            link.Attributes.Add("href", page.ClientScript.GetWebResourceUrl(typeof(Values), "umbraco.editorControls.SettingControls.css.Values.css"));
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("rel", "stylesheet");
            head.Controls.Add(link);

   
            return p;
        }
    }
}
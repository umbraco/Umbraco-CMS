using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using ClientDependency.Core.Controls;
using ClientDependency.Core;

namespace umbraco.cms.businesslogic.skinning.controls
{
    [ClientDependency(430, ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency(440, ClientDependencyType.Javascript, "LiveEditing/Modules/SkinModule/js/initslider.js", "UmbracoRoot")]
    [ClientDependency(ClientDependencyType.Css, "ui/ui-lightness/jquery-ui-1.8.4.custom.css", "UmbracoClient")]
    public class SliderControl: TextBox
    {
        public int MinimumValue { get; set; }
        public int MaximumValue { get; set; }
        public int InitialValue { get; set; }
        public int Ratio { get; set; }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            this.Attributes.Add("style", "display:none;");

            base.Render(writer);

            writer.WriteLine(
                string.Format(
                "<div class='skinningslider' rel='{0}'></div>",
                MinimumValue + "," + MaximumValue + "," + InitialValue + "," +  Ratio +","+this.ClientID));

           
        }
    }
}

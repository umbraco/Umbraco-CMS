using System.ComponentModel;
using System.Web.UI;

namespace umbraco.uicontrols {
    internal class Splitter : System.Web.UI.WebControls.Image {

        protected override void OnLoad(System.EventArgs EventArguments) {
            this.Height = System.Web.UI.WebControls.Unit.Pixel(21);
            this.Style.Add("border", "0px");
            this.Attributes.Add("class", "editorIconSplit");
            this.ImageUrl = "/umbraco_client/menuicon/images/split.gif";
        }
    }
}
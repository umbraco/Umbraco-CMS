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

namespace umbraco.uicontrols {
    public class MenuImageButton : System.Web.UI.WebControls.ImageButton, MenuIconI {
        private string _OnClickCommand = "";


        public override string ID {
            get { return base.ID; }
            set { base.ID = value; }
        }

        public string AltText {
            get { return this.AlternateText; }
            set {
                this.AlternateText = value;
                this.Attributes.Add("title", value);
            }
        }
        public int IconWidth {
            get { return (int)this.Width.Value; }
            set { this.Width = value; }
        }
        public int IconHeight {
            get { return (int)this.Height.Value; }
            set { this.Height = value; }
        }


        public string ImageURL {
            get { return base.ImageUrl; }
            set { base.ImageUrl = value; }
        }

        public string OnClickCommand {
            get { return _OnClickCommand; }
            set { _OnClickCommand = value; }
        }

        protected override void OnLoad(System.EventArgs EventArguments) {
            SetupClientScript();

            this.Width = Unit.Pixel(22);
            this.Height = Unit.Pixel(23);
            this.Style.Add("border", "0px");
            this.Attributes.Add("class", "editorIcon");
            this.Attributes.Add("onMouseover", "this.className='editorIconOver'");
            this.Attributes.Add("onMouseout", "this.className='editorIcon'");
            this.Attributes.Add("onMouseup", "this.className='editorIconOver'");
            this.Attributes.Add("onMouseDown", "this.className='editorIconDown'");

            if (_OnClickCommand != "") {
                this.Attributes.Add("onClick", OnClickCommand);
            }
        }

        private void SetupClientScript() {
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "MENUICONCSS", "<link rel='stylesheet' href='/umbraco_client/menuicon/style.css' />");
        }
    }
}
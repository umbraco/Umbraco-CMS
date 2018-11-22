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
using ClientDependency.Core;

namespace umbraco.uicontrols {

    public class MenuImageButton : System.Web.UI.WebControls.ImageButton, MenuIconI {
        private string _OnClickCommand = "";

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

        protected override void CreateChildControls()
        {
            this.Width = Unit.Pixel(20);
            this.Height = Unit.Pixel(20);
            this.Style.Clear();
            this.Attributes.Add("class", "btn btn-default editorIcon");


            if (_OnClickCommand != "")
            {
                this.Attributes.Add("onClick", OnClickCommand);
            }
            base.CreateChildControls();
        }
    }
}
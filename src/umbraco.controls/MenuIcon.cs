using System.ComponentModel;
using System.Web.UI;
using ClientDependency.Core;

namespace umbraco.uicontrols {

	[ClientDependency(ClientDependencyType.Css, "menuicon/style.css", "UmbracoClient")]
    internal class MenuIcon : System.Web.UI.WebControls.Image, MenuIconI 
	{
        private string _OnClickCommand = "";
        private string _AltText = "init";


        public string ID1 {
            get { return this.ID; }
            set { this.ID = value; }
        }

        public string AltText {
            get { return this.AlternateText; }
            set {
                _AltText = value;
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
            get { return this.ImageUrl; }
            set { this.ImageUrl = value; }
        }

        public string OnClickCommand {
            get { return _OnClickCommand; }
            set { _OnClickCommand = value; }
        }

        protected override void OnLoad(System.EventArgs EventArguments) {


            // NH 17-01-2007. Trying to avoid inline styling soup 
            //        Me.Width = WebControls.Unit.Pixel(22)
            //       Me.Height = WebControls.Unit.Pixel(23)
            //Me.Style.Add("border", "0px")
            this.Attributes.Add("class", "editorIcon");
            this.Attributes.Add("onMouseover", "this.className='editorIconOver'");
            string holder = "";
//            if (this.ID != "") {
                //holder = this.ID.Substring(0, this.ID.LastIndexOf("_")) + "_menu";
                this.Attributes.Add("onMouseout", "hoverIconOut('" + holder + "','" + this.ID + "');");
                this.Attributes.Add("onMouseup", "hoverIconOut('" + holder + "','" + this.ID + "');");
//            } else {
                this.Attributes.Add("onMouseout", "this.className='editorIcon'");
                this.Attributes.Add("onMouseup", "this.className='editorIcon'");
//            }
            this.Attributes.Add("onMouseDown", "this.className='editorIconDown'; return false;");
            this.AlternateText = _AltText;
            this.Attributes.Add("title", _AltText);

            if (_OnClickCommand != "") {
                this.Attributes.Add("onClick", OnClickCommand);
            }
        }


    }
}
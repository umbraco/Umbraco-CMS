using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.uicontrols
{
    public class MenuButton : System.Web.UI.WebControls.LinkButton
    {
        public MenuButtonType ButtonType { get; set; }
        public DataAttributes Data { get; private set; }
        public string Hotkey { get; set; }

        public string Icon { get; set; }
        
        public MenuButton()
        {
            Data = new DataAttributes();
            CssClass = "btn";
        }


        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            //setup a hotkey if present
            if (ButtonType == MenuButtonType.Primary && String.IsNullOrEmpty(Hotkey))
                Data.Add("shortcut", "ctrl+" + this.Text.ToLower()[0]);
            else if (!String.IsNullOrEmpty(Hotkey))
                Data.Add("shortcut", Hotkey);

            Data.AppendTo(this);

            string cssClass = "btn";
            
            if (Icon != null)
            {
                cssClass = "btn editorIcon";
                var i = Icon.Trim('.');

                if (!string.IsNullOrEmpty(i))
                {
                    this.ToolTip = this.Text;

                    if (i.Contains("."))
                        this.Text = "<img src='" + umbraco.IO.IOHelper.ResolveUrl(Icon) + "' alt='" + this.ToolTip + "'/> " + this.ToolTip;
                    else
                        this.Text = "<i class='icon icon-" + Icon.Replace("icon-", "") + "'></i> " + this.ToolTip;
                }
            }

            cssClass += " btn-" + Enum.GetName(ButtonType.GetType(), ButtonType).ToLower();
            this.CssClass = cssClass;


            base.Render(writer);
        }

    }


    public enum MenuButtonType
    {
        Default,
        Primary,
        Info,
        Success,
        Warning,
        Danger,
        Inverse,
        Link
    }
}

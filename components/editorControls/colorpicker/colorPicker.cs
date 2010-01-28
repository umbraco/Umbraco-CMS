using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using umbraco.IO;

namespace umbraco.editorControls
{
    /// <summary>
    /// Summary description for colorPicker.
    /// </summary>
    [DefaultProperty("Value"),
    ValidationProperty("Value"),
    ToolboxData("<{0}:colorPicker runat=server></{0}:colorPicker>")]
    public class colorPicker : System.Web.UI.WebControls.HiddenField, interfaces.IDataEditor
    {
        private interfaces.IData _data;
        private SortedList _prevalues;
        
        public colorPicker(interfaces.IData Data, SortedList Prevalues)
        {
            _data = Data;
            _prevalues = Prevalues;
        }
        private ArrayList _colors = new ArrayList();

        public Control Editor { get { return this; } }

        public colorPicker()
        {
        }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }
        public bool ShowLabel
        {
            get { return true; }
        }

        public void Save()
        {
            _data.Value = this.Value;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

		  if (_data != null && _data.Value != null)
               this.Value = _data.Value.ToString();

            _colors.Add(new ListItem(""));

            try
            {
                foreach (string val in _prevalues.Values)
                {
                    _colors.Add(new ListItem(val));
                }
            }
            catch { }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            string _bgColor = this.Value;
            if (_bgColor == "")
                _bgColor = "FFF";
		
			base.Render(writer);
            writer.WriteLine("<div style=\"clear:both;padding-top:5px\"><span id=\"" + this.ClientID + "holder\" style=\"border: 1px solid black; background-color: #" + _bgColor + "\"><img src=\"" + SystemDirectories.Umbraco + "/images/nada.gif\" width=\"15\" height=\"15\" border=\"0\" /></span>&nbsp; - ");

            foreach (object color in _colors)
            {
                string colorValue = color.ToString();
                if (colorValue == "")
                    colorValue = "FFF";

                writer.WriteLine("<span style=\"margin: 2px; border: 1px solid black; background-color: #" + color.ToString() + "\"><a href=\"javascript:void(0);\" onClick=\"document.forms[0]['" + this.ClientID + "'].value = '" + color.ToString() + "'; document.getElementById('" + this.ClientID + "holder').style.backgroundColor = '#" + colorValue + "'\"><img src=\"" + SystemDirectories.Umbraco + "/images/nada.gif\" width=\"15\" height=\"15\" border=\"0\" /></a></span>");
            }

            writer.WriteLine("</div>");
        }
    }
}

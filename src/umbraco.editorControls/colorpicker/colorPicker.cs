using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using Umbraco.Core.IO;
using System.Collections.Generic;

namespace umbraco.editorControls
{
    /// <summary>
    /// Summary description for colorPicker.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class colorPicker : System.Web.UI.WebControls.HiddenField, interfaces.IDataEditor
    {
        private interfaces.IData _data;
        private SortedList _prevalues;
        
        public colorPicker(interfaces.IData Data, SortedList Prevalues)
        {
            _data = Data;
            _prevalues = Prevalues;
        }

        List<KeyValuePair<int, String>> Prevalues;
        public colorPicker(interfaces.IData Data, List<KeyValuePair<int, String>> Prevalues)
        {
            _data = Data;
            this.Prevalues = Prevalues;
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
                if (_prevalues != null)
                {
                    foreach (string val in _prevalues.Values)
                    {
                        _colors.Add(new ListItem(val));
                    }
                }
                else if (Prevalues != null)
                {
                    foreach (KeyValuePair<int, String> item in Prevalues)
                    {
                        _colors.Add(new ListItem(item.Value));
                    }
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
            writer.WriteLine("<div style=\"clear:both;padding-top:5px\"><span id=\"" + this.ClientID + "holder\" style=\"border: 1px solid black; background-color: #" + _bgColor + "\"><img src=\"" + this.ResolveUrl(SystemDirectories.Umbraco) + "/images/nada.gif\" width=\"15\" height=\"15\" border=\"0\" /></span>&nbsp; - ");

            foreach (object color in _colors)
            {
                string colorValue = color.ToString();
                if (colorValue == "")
                    colorValue = "FFF";

                writer.WriteLine("<span style=\"margin: 2px; border: 1px solid black; background-color: #" + color.ToString() + "\"><a href=\"javascript:void(0);\" onClick=\"document.forms[0]['" + this.ClientID + "'].value = '" + color.ToString() + "'; document.getElementById('" + this.ClientID + "holder').style.backgroundColor = '#" + colorValue + "'\"><img src=\"" + this.ResolveUrl(SystemDirectories.Umbraco) + "/images/nada.gif\" width=\"15\" height=\"15\" border=\"0\" /></a></span>");
            }

            writer.WriteLine("</div>");
        }
    }
}

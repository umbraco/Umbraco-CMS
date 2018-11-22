using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace umbraco.editorControls.checkboxlist
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class checkboxlistEditor : System.Web.UI.WebControls.CheckBoxList, interfaces.IDataEditor	
	{
		private String _text;

		interfaces.IData _data;
		SortedList _prevalues;
		public checkboxlistEditor(interfaces.IData Data,SortedList Prevalues)
		{
			_data = Data;
			_prevalues = Prevalues;
		}

        List<KeyValuePair<int, String>> Prevalues;
        public checkboxlistEditor(interfaces.IData Data, List<KeyValuePair<int, String>> Prevalues)
        {
            _data = Data;
            this.Prevalues = Prevalues;
        }

		public Control Editor { get {return this;}}

		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}
		public virtual bool ShowLabel 
		{
			get {return true;}
		}

		public void Save() 
		{
			_text = "";
			foreach(ListItem li in base.Items) 
			{
				if (li.Selected) 
				{
					_text += li.Value + ",";
				}
			}

			if (_text.Length > 0)
				_text = _text.Substring(0, _text.Length-1);
			_data.Value = _text;
			
			
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);

			if (_data != null && _data.Value != null && _data.Value.ToString() != "")
				_text = _data.Value.ToString();

            if (_prevalues != null)
            {
                foreach (object key in _prevalues.Keys)
                {
                    ListItem li = new ListItem(_prevalues[key].ToString(), key.ToString());

                    if (("," + _text + ",").IndexOf("," + li.Value.ToString() + ",") > -1)
                        li.Selected = true;

                    Items.Add(li);
                }
            }
            else if (Prevalues != null)
            {
                foreach (KeyValuePair<int, String> item in Prevalues)
                {
                    ListItem li = new ListItem(item.Value, item.Key.ToString());

                    if (("," + _text + ",").IndexOf("," + li.Value.ToString() + ",") > -1)
                        li.Selected = true;

                    Items.Add(li);
                }
            }

		}
	}
}

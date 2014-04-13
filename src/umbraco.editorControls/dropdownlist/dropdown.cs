using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;

using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using System.Collections.Generic;

namespace umbraco.editorControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class dropdown : System.Web.UI.WebControls.DropDownList, interfaces.IDataEditor
	{
		private interfaces.IData _data;
		private SortedList _prevalues;

		public dropdown(interfaces.IData Data, SortedList Prevalues) 
		{
			_data = Data;
			_prevalues = Prevalues;
		}


        List<KeyValuePair<int, String>> Prevalues;
        public dropdown(interfaces.IData Data, List<KeyValuePair<int, String>> Prevalues)
        {
            _data = Data;
            this.Prevalues = Prevalues;
        }


		public Control Editor {
			get {return this;}
		}

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
			string tmpVal = "";
			if (this.SelectedIndex > 0)
				tmpVal = this.SelectedValue;
			_data.Value = tmpVal;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);

            if (_prevalues != null)
            {
                foreach (object key in _prevalues.Keys)
                {
                    this.Items.Add(new ListItem(Dictionary.ReplaceKey(_prevalues[key].ToString()), key.ToString()));
                }

            }
            else if (Prevalues != null)
            {
                foreach (KeyValuePair<int, String> item in Prevalues)
                {
                    this.Items.Add(new ListItem(Dictionary.ReplaceKey(item.Value), item.Key.ToString()));
                }
            }

			base.Items.Insert(0, new ListItem(ui.Text("choose") + "...",""));

			if (_data != null && _data.Value != null)
				this.SelectedValue = _data.Value.ToString();			
		}
	}
}
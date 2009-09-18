using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;

namespace umbraco.editorControls
{
	public class dropdown : System.Web.UI.WebControls.DropDownList, interfaces.IDataEditor
	{
		private interfaces.IData _data;
		private SortedList _prevalues;

		public dropdown(interfaces.IData Data, SortedList Prevalues) 
		{
			_data = Data;
			_prevalues = Prevalues;
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
            foreach (object key in _prevalues.Keys)
            {
				this.Items.Add(new ListItem(_prevalues[key].ToString(),key.ToString()));						
			}
			base.Items.Insert(0, new ListItem(ui.Text("choose") + "...",""));

			if (_data != null && _data.Value != null)
				this.SelectedValue = _data.Value.ToString();			
		}
	}
}
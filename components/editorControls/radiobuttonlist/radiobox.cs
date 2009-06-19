using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
namespace umbraco.editorControls
{
	public class radiobox : System.Web.UI.WebControls.RadioButtonList, interfaces.IDataEditor
	{
		private interfaces.IData _data;
		private SortedList _prevalues;

		public radiobox(interfaces.IData Data, SortedList Prevalues) 
		{
			_data = Data;
			_prevalues = Prevalues;
		}

		public Control Editor 
		{
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
			
			try {
				if (_data != null && _data.Value != null)
					this.SelectedValue = _data.Value.ToString();	
			} catch {}			
		}
	}
}

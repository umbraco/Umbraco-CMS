using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;

namespace umbraco.editorControls.textfield
{
	public class TextFieldEditor : TextBox, interfaces.IDataEditor
	{
		private interfaces.IData _data;


		public TextFieldEditor(interfaces.IData Data) {
			_data = Data;
		}

		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}

		public bool ShowLabel 
		{
			get {return true;}
		}

		public Control Editor {get{return this;}}

		public void Save() 
		{
			_data.Value = this.Text;
		}

		protected override void OnInit(EventArgs e)
		{
			if (this.CssClass == "")
				this.CssClass = "umbEditorTextField";
	
			if (_data != null && _data.Value != null) 
				Text = _data.Value.ToString();
	
			base.OnInit(e);
		}
	}
}
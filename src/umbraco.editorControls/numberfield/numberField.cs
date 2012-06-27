using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace umbraco.editorControls
{
	public class numberField : System.Web.UI.WebControls.TextBox, interfaces.IDataEditor
	{
		private interfaces.IData _data;
		public numberField(interfaces.IData Data) {
			_data = Data;
		}

	
		public Control Editor {
			get{return this;}
	
		}
		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}
		public bool ShowLabel 
		{
			get {return true;}
		}
		
		public void Save() 
		{
            if (Text.Trim() != "")
			    _data.Value = Text;
            else
                _data.Value = null;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
            CssClass = "umbEditorNumberField";
			// load data
			if (_data != null && _data.Value != null)
				this.Text = _data.Value.ToString();
		}

		public override string Text
		{
			get { return base.Text; }
			set 	{
				try 	{
					if (value != null)
					    base.Text = Convert.ToInt32(value).ToString();
				} 
				catch {
					base.Text = "";
					System.Web.HttpContext.Current.Trace.Warn("Numberfield", "Value has to be an integer (" + value + ")");
				}
			}
		}

	}
}

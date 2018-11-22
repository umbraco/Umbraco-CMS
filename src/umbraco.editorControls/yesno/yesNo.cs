using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace umbraco.editorControls
{
	/// <summary>
	/// Generates a radiolist of yes and no for boolean fields
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class yesNo : System.Web.UI.WebControls.CheckBox, interfaces.IDataEditor
	{
	
		private interfaces.IData _data;
		public yesNo(interfaces.IData Data) {
			_data = Data;
		}
		private String _text;

		public Control Editor {
			get 
			{
				return this;
			}
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
				string value = "";
				if (Checked)
					value = "1";
				else
					value = "0";
			_data.Value = value;
			
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
            this.Text = ui.Text("yes");
		}

	
		/// <summary> 
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void Render(HtmlTextWriter output)
		{
            // zb-00024 #299260 : always render, postback or not postback
            if (_data != null && _data.Value != null)
            {
                if (_data.Value.ToString() == "1")
                    this.Checked = true;
            }

			base.Render(output);
		}
	}
}

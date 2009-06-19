using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using System.Globalization;

namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for dateField.
	/// </summary>
	[DefaultProperty("Text"), 
	ToolboxData("<{0}:dateField runat=server></{0}:dateField>")]
	public class dateField : controls.datePicker, interfaces.IDataEditor
	{

		interfaces.IData _data;

		public dateField(interfaces.IData Data) {
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

		public Control Editor {
			get {return this;}
		}

		public void Save() 
		{
			try 
			{
				if (this.Text == String.Empty) 
                    throw new FormatException();

				DateTime date = DateTime.Parse(this.Text);
                this.Text = date.ToString("yyyy-MM-dd") + " " + date.ToLongTimeString();
                _data.Value = date;
			} 
			catch {
				this.Text = "";
				_data.Value = null;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			//base.ShowTime = false;
			base.CustomMinutes = "00, 05, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55";
			
			if (_data != null && _data.Value != null && _data.Value.ToString() != "")
				this.Text = _data.Value.ToString();
			else {
				base.EmptyDateAsDefault = true;
				this.Text = "";
			}
			base.OnInit(e);
		}
	}
}

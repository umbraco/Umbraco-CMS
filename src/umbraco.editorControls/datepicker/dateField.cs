using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using umbraco.uicontrols.DatePicker;

namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for dateField.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class dateField : DateTimePicker, interfaces.IDataEditor
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

				//DateTime date = DateTime.Parse(this.Text);
                //this.Text = date.ToString("yyyy-MM-dd") + " " + date.ToLongTimeString();
                //_data.Value = date;
                _data.Value = this.DateTime;
			} 
			catch {
				//this.Text = "";
				_data.Value = null;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			//base.ShowTime = false;
			//base.CustomMinutes = "00, 05, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55";

            if (_data != null && _data.Value != null && _data.Value is DateTime)
            {
                this.DateTime = (DateTime)_data.Value;
            }
            else
            {
                //base.EmptyDateAsDefault = true;
                //this.Text = "";
            }
			base.OnInit(e);
		}
	}
}

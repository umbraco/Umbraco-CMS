using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

namespace umbraco.editorControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class numberField : TextBox, interfaces.IDataEditor
	{
		private interfaces.IData _data;

		public numberField(interfaces.IData Data) 
        {
			_data = Data;
		}
	
		public Control Editor 
        {
			get
            {
                return this;
            }	
		}

		public virtual bool TreatAsRichTextEditor 
		{
			get 
            {
                return false;
            }
		}

		public bool ShowLabel 
		{
			get 
            {
                return true;
            }
		}
		
		public void Save() 
		{
            if (Text.Trim() != "")
            {
                _data.Value = Text;
            }
            else
            {
                _data.Value = null;
            }
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
            
            this.CssClass = "umbEditorNumberField";

			// load data
            if (_data != null && _data.Value != null)
            {
                this.Text = _data.Value.ToString();
            }
		}

        /// <summary>
        /// The setter ensures that only valid integers are saved - this is to prevent invalid types from being saved into an int db field
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                if (value != null)
                {
                    base.Text = "";//Resets the text-field in case the value is removed

                    int integer;//The value will only be parsed if it contains a valid value
                
                if (int.TryParse(value, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out integer))
                    {
                        base.Text = integer.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }
	}
}

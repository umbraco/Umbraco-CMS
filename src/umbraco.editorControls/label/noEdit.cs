using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.editorControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class noEdit : System.Web.UI.WebControls.Label, interfaces.IDataEditor
	{
		private interfaces.IData _data;
        private bool labelRefreshed = false;

		public noEdit(interfaces.IData Data) {
			_data = Data;
		}
		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}
		public bool ShowLabel {get {return true;}}

		public void Save() 
		{
            // Uncommented this as a label should *never* update itself via ui
//			_data.Value = this.Text;		
		}

        public void RefreshLabel(string content)
        {
            Text = content;
            labelRefreshed = true;
        }

		public Control Editor {get {return this;}}

		protected override void OnInit(EventArgs e)
		{
            if (!labelRefreshed && (_data != null && _data.Value != null))
				Text = _data.Value.ToString();

			base.OnInit(e);
		}

		//public override string Text {
		//     get {return _data.Value.ToString();}
		//}
	}
}
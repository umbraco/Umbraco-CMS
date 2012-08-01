using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using System.Web.UI.WebControls;
using System.Drawing;

namespace umbraco.editorControls.tinymce
{
	class TinyMCELegacyControl : WebControl, IDataEditor, IDataPrevalue
	{

		protected Label lbl;

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			lbl = new Label();
			lbl.ID = "InvalidDataTypeLabel";
			lbl.Text = "This is an old version of the Richtext Editor (TinyMCE2), which is deprecated and no longer supported in Umbraco 4. Please upgrade by going to >> Developer >> Data Types >> Richtext editor >> And choose \"TinyMCE3 wysiwyg\" as the rendercontrol. If you don't have administrative priviledges in umbraco, you should contact your administrator";
			lbl.ForeColor = Color.Red;
			lbl.Font.Bold = true;

			this.Controls.Add(lbl);
		}


		#region IDataEditor Members

		public void Save()
		{
			
		}

		public bool ShowLabel
		{
			get { return false; }
		}

		public bool TreatAsRichTextEditor
		{
			get { return false; }
		}

		public System.Web.UI.Control Editor
		{
			get
			{
				return this;
			}
		}

		#endregion

		#region IDataPrevalue Members

		void IDataPrevalue.Save()
		{
			
		}

		System.Web.UI.Control IDataPrevalue.Editor
		{
			get { return this; }
		}

		#endregion
	}
}

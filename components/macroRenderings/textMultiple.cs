using System;

namespace umbraco.macroRenderings
{
	/// <summary>
	/// Summary description for textMultiple.
	/// </summary>
	public class textMultiple : text
	{
		public textMultiple()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
			this.TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine;
			this.Rows = 6;
		}

	}
}

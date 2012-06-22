using System;

namespace umbraco.macroRenderings
{
	/// <summary>
	/// Summary description for contentTypeMultiple.
	/// </summary>
	public class contentTypeMultiple : contentTypeSingle
	{
		public contentTypeMultiple()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		protected override void OnInit(EventArgs e)
		{
			this.Multiple = true;
			base.OnInit (e);
		}

	}
}

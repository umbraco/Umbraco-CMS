using System;

namespace umbraco.macroRenderings
{
	/// <summary>
	/// Summary description for tabPickerMultiple.
	/// </summary>
	public class tabPickerMultiple : tabPicker
	{
		public tabPickerMultiple()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		protected override void OnInit(EventArgs e)
		{
			base.Multiple = true;
			base.OnInit (e);
		}

	}
}

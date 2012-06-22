using System;

namespace umbraco.macroRenderings
{
	/// <summary>
	/// Summary description for propertyTypePickerMultiple.
	/// </summary>
	public class propertyTypePickerMultiple : propertyTypePicker
	{
		public propertyTypePickerMultiple()
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

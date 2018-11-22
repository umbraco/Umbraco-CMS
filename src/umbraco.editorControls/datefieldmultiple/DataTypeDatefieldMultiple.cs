using System;
using Umbraco.Core;

namespace umbraco.editorControls.datefieldmultiple
{
	/// <summary>
	/// Summary description for DataTypeDatefieldMultiple.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class DataTypeDatefieldMultiple : datepicker.DateDataType
	{
		private interfaces.IDataEditor _Editor;
		
		public override Guid Id
		{
			get 
			{
				return new Guid(Constants.PropertyEditors.DateTime);
			}
		}

		public override string DataTypeName
		{
			get
			{
				return "Date/Time";
			}
		}
		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null) 
				{
					dateField df = new dateField(Data);
					df.ShowTime = true;
					_Editor = df;
				}
				
				return _Editor;
			}
		}

	}
}

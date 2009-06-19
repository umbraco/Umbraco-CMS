using System;

namespace umbraco.editorControls.datefieldmultiple
{
	/// <summary>
	/// Summary description for DataTypeDatefieldMultiple.
	/// </summary>
	public class DataTypeDatefieldMultiple : datepicker.DateDataType
	{
		private interfaces.IDataEditor _Editor;
		
		public override Guid Id
		{
			get 
			{
				return new Guid("B6FB1622-AFA5-4BBF-A3CC-D9672A442222");
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

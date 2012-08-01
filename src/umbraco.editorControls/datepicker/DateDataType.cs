using System;

namespace umbraco.editorControls.datepicker
{
	/// <summary>
	/// Summary description for DateDataType.
	/// </summary>
	public class DateDataType : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null) 
				{
					dateField df = new dateField(Data);
					df.ShowTime = false;
					_Editor = df;
				}
				return _Editor;
			}
		}

		public override interfaces.IData Data 
		{
			get 
			{
				if (_baseData == null)
					_baseData = new DateData(this);
				return _baseData;
			}
		}


		public override string DataTypeName 
		{
			get {return "Date";}
		}

		public override Guid Id 
		{
			get {return new Guid("23e93522-3200-44e2-9f29-e61a6fcbb79a");}
		}

		public override interfaces.IDataPrevalue PrevalueEditor 
		{
			get 
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new DefaultPrevalueEditor(this,false);
				return _prevalueeditor;
			}
		}
	}
}

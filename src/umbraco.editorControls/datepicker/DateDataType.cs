using System;
using Umbraco.Core;

namespace umbraco.editorControls.datepicker
{
	/// <summary>
	/// Summary description for DateDataType.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
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
			get { return new Guid(Constants.PropertyEditors.Date); }
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

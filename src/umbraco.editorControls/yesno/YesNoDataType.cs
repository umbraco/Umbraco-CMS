using System;

namespace umbraco.editorControls.yesno
{
	/// <summary>
	/// Summary description for YesNoDataType.
	/// </summary>
	public class YesNoDataType : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null)
					_Editor = new yesNo(Data);
				return _Editor;
			}
		}

		public override interfaces.IData Data 
		{
			get 
			{
				if (_baseData == null)
					_baseData = new cms.businesslogic.datatype.DefaultData(this);
				return _baseData;
			}
		}

		public override string DataTypeName
		{
			get {return "True/False (Ja/Nej)";}
		}
		public override Guid Id 
		{
			get {return new Guid("38b352c1-e9f8-4fd8-9324-9a2eab06d97a");}
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
